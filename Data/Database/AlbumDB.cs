using System.ComponentModel.DataAnnotations;

namespace SpotifyR
{
    public class AlbumDB
    {
        [Key]
        public string id { get; set; }
        public string name { get; set; }
        public string uri { get; set; }     
    }
}