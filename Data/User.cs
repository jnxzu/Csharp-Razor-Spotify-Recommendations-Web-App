using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SpotifyR.Data
{
    public class User
    {
        public string display_name { get; set; }
        public string href { get; set; }
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        //public Image[] images { get; set; }
        public string product { get; set; }
        public string uri { get; set; }
    }
}