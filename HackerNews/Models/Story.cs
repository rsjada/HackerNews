namespace HackerNews.Models
{
    public class Story
    {

        public string title { get; set; }
        public string url { get; set; }
        public string by { get; set; }
        public int descendants { get; set; }
        public int score { get; set; }
        public long time { get; set; } // Add a new property for the Unix timestamp

    }
}