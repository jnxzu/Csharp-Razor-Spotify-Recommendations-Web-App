namespace SpotifyR.Data
{
    public class Track
    {
        public Album album { get; set; }
        public Artist[] artists { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public int popularity { get; set; }
        public string preview_url { get; set; }
        public string uri { get; set; }
        public int duration_ms { get; set; }
        public int duration
        {
            get => duration_ms / 1000;
        }
    }
}