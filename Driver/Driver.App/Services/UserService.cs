using MongoDB.Driver;
using Driver.App.Model;

namespace Driver.Service
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

            return user ?? null ;
        }
        public async Task<List<User>> GetAsync() =>
           await _mongoService.Users.Find(_ => true).ToListAsync();

        public async Task<User> GetUserAsync(string id) => await _mongoService.Users.Find(p => p.Id == id).FirstOrDefaultAsync();
    }
}
