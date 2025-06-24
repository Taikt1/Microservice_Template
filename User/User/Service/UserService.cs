using MongoDB.Driver;
using TestKafka.Model;

namespace TestKafka.Service
{
    public class UserService
    {
        private readonly MongoContext _mongoService;

        public UserService(MongoContext mongoService)
        {
            _mongoService = mongoService;
        }

        public async Task<User> Login(string email, string password)
        {
            var user = await _mongoService.Users
        .Find(u => u.Email == email && u.Password == password)
        .FirstOrDefaultAsync();

            return user ?? null;
        }
    }
}
