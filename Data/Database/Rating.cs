using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SpotifyR
{
    public class Rating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public bool value { get; set; }
        public string userId { get; set; }
        public string artistId { get; set; }
        public UserDB user { get; set; }
        public ArtistDB artist { get; set; }
    }
}