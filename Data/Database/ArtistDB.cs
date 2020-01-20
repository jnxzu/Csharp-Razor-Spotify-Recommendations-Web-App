using System.ComponentModel.DataAnnotations;

namespace SpotifyR
{
    public class ArtistDB
    {        
        [Key]
        public string id { get; set; }   
        public string name { get; set; }
        public string uri { get; set; }
    }
}