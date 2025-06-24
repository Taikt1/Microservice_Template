using MongoDB.Driver;
using TestKafka.Model;
using User.Model;

namespace TestKafka.Service
{
    public class UserService
    {
        private readonly MongoContext _mongoService;

        public UserService(MongoContext mongoService)
        {
            _mongoService = mongoService;
        }

        public async Task<ApplicationUser> Login(string email, string password)
        {
            var user = await _mongoService.Users
        .Find(u => u.Email == email && u.Password == password)
        .FirstOrDefaultAsync();

            return user ?? null;
        }

        public async Task<ApplicationUser> CreateUser(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            await _mongoService.Users.InsertOneAsync(user);
            return user;
        }

        public async Task<ApplicationUser?> GetUserById(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Id cannot be null or empty", nameof(id));
            return await _mongoService.Users
                .Find(u => u.Id == id)
                .FirstOrDefaultAsync();
        }

        

    }
}
