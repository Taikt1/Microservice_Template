using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace User.Model
{
    public class ApplicationUser
    {
        [BsonId] // ⬅ Gắn để Mongo biết đây là _id
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
    }
}
