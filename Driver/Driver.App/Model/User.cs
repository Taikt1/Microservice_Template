using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace Driver.App.Model
{
    public class User
    {
        [BsonId] // ⬅ Gắn để Mongo biết đây là _id
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonIgnore] // ⬅ Bỏ qua khi serialize sang JSON
        public string? Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }

    }
}
