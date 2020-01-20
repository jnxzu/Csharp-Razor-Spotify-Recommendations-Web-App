using System.ComponentModel.DataAnnotations;

namespace SpotifyR
{
    public class TrackDB
    {
        [Key]
        public string id { get; set; }
        public string name { get; set; }
        public string uri { get; set; }
    }
}