using HackerNews.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HackerNews.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StoriesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoryDTO>>> GetBestStories(int StoryID, int NumberOfBestStories)
        {
            var httpClient = _httpClientFactory.CreateClient();

            // Fetching a sample story to get the author's name
            var sampleStoryResponse = await httpClient.GetAsync("https://hacker-news.firebaseio.com/v0/item/" + StoryID + ".json");
            if (!sampleStoryResponse.IsSuccessStatusCode)
                return BadRequest("Failed to retrieve a sample story from Hacker News API.");

            var sampleStoryJson = await sampleStoryResponse.Content.ReadAsStringAsync();
            var sampleStory = JsonSerializer.Deserialize<Story>(sampleStoryJson);

            // Extracting the author's name from the sample story
            var authorName = sampleStory.by;
            var descendants = sampleStory.descendants;

            // Fetch the IDs of the best stories
            var bestStoriesIdsResponse = await httpClient.GetAsync("https://hacker-news.firebaseio.com/v0/beststories.json");
            if (!bestStoriesIdsResponse.IsSuccessStatusCode)
                return BadRequest("Failed to retrieve best stories IDs from Hacker News API.");

            var bestStoriesIdsJson = await bestStoriesIdsResponse.Content.ReadAsStringAsync();
            var bestStoriesIds = JsonSerializer.Deserialize<List<int>>(bestStoriesIdsJson);

            var bestStoriesTasks = new List<Task<StoryDTO>>();

            foreach (var storyId in bestStoriesIds)
            {
                bestStoriesTasks.Add(GetStoryDetailsAsync(httpClient, storyId, authorName, descendants));
            }

            var bestStories = new List<StoryDTO>();
            while (bestStoriesTasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(bestStoriesTasks);
                bestStoriesTasks.Remove(completedTask);
                var story = await completedTask;
                if (story != null)
                {
                    bestStories.Add(story);
                }
            }

            // Sort the stories by score in descending order
            bestStories.Sort((x, y) => y.score.CompareTo(x.score));

            // Take only the top n stories
            bestStories = bestStories.GetRange(0, NumberOfBestStories);

            return bestStories;
        }

        private async Task<StoryDTO> GetStoryDetailsAsync(HttpClient httpClient, int storyId, string authorName, int descendants)
        {
            var storyDetailsResponse = await httpClient.GetAsync($"https://hacker-news.firebaseio.com/v0/item/{storyId}.json");
            if (!storyDetailsResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var storyDetailsJson = await storyDetailsResponse.Content.ReadAsStringAsync();
            var storyDetails = JsonSerializer.Deserialize<Story>(storyDetailsJson);

            // Create a new StoryDTO object and populate its properties
            var storyDTO = new StoryDTO
            {
                title = storyDetails.title,
                uri = storyDetails.url,
                postedBy = authorName,
                score = storyDetails.score,
                time = DateTimeOffset.FromUnixTimeSeconds(storyDetails.time).ToString("yyyy-MM-dd HH:mm:ss"),
                commentCount = descendants

            };

            return storyDTO;
        }

    }
}


