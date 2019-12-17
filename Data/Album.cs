namespace SpotifyR.Data
{
    public class Album
    {
        public Artist[] artists { get; set; }
        public string[] genres { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public Image[] images { get; set; }
        public string release_date { get; set; }
        public string release_date_precision { get; set; }
        public PagingTrack tracks { get; set; }
        public string uri { get; set; }
        public string name { get; set; }
    }
}