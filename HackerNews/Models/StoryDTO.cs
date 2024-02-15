namespace HackerNews.Models
{
    public class StoryDTO
    {
        public string title { get; set; }
        public string uri { get; set; }
        public string postedBy { get; set; }
        public int score { get; set; }
        public string time { get; set; }
        public int commentCount { get; set; }
        // public string FormattedTime { get; set; } // Added FormattedTime property

    }
}
