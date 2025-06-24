using MongoDB.Driver;

namespace Order.App.Model
{
    public class MongoContext
    {
        private readonly IMongoDatabase _database;
        public MongoContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["ConnectionStrings:MongoDB"]);
            _database = client.GetDatabase(configuration["ConnectionStrings:DatabaseName"]);

        }

        public IMongoCollection<Trip> Trips => _database.GetCollection<Trip>("Trip");

    }
}
