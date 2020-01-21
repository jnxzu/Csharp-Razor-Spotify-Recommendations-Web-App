using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SpotifyR
{
    public class ArtistDB
    { 
        [Key]
        public string id { get; set; }  
        public ICollection<Rating> ratings { get; set; }
 
    }
}