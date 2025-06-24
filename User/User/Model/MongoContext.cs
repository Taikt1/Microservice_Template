using MongoDB.Driver;

namespace TestKafka.Model
{
    public class MongoContext
    {
        private readonly IMongoDatabase _database;
        public MongoContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["ConnectionStrings:MongoDB"]);
            _database = client.GetDatabase(configuration["ConnectionStrings:DatabaseName"]);

        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("User");

    }
}
