using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SpotifyR
{
    public class UserDB
    {
        [Key]
        public string id { get; set; }
        public string uri { get; set; }
        public ICollection<Rating> ratings { get; set; }
    }
}