namespace SpotifyR
{
    public class Album
    {
        public string album_type { get; set; }
        public string id { get; set; }
        public string release_date { get; set; }
        public string release_date_precision { get; set; }
        public PagingTrack tracks { get; set; }
        public string name { get; set; }
    }
}