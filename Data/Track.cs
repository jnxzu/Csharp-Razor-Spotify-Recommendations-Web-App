namespace SpotifyR
{
    public class Track
    {
        public Album album { get; set; }
        public Artist[] artists { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public int popularity { get; set; }
        public string uri { get; set; }
    }
}