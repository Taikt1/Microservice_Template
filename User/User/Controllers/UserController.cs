using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using TestKafka.Model;
using TestKafka.Redis;
using TestKafka.Service;
using MongoDB.Bson;
using Microsoft.AspNetCore.DataProtection.KeyManagement;


namespace TestKafka.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly UserService _userService;
        private readonly KafkaProducerService<string, Trip> _producer;
        private readonly IResponseCache _responseCache;

        public UserController(UserService userService, KafkaProducerService<string, Trip> producer, IResponseCache responseCache)
        {
            _userService = userService;
            _producer = producer;
            _responseCache = responseCache;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Xử lý đăng nhập ở đây
            var login = await _userService.Login(request.Email, request.Password);
            if(login != null)
            {
                return Ok(new { status = login });
            }
            return BadRequest("Login Failed");
        }

        [HttpPost("create-trip")]
        public async Task<IActionResult> Create([FromBody] TripRequest trip)
        {

            var evt = new Trip
            {
                Id = ObjectId.GenerateNewId().ToString(), // tạo ID mới,
                CustomerId = trip.UserId,
                DriverId = null, // Chưa có ai nhận
                PickupLocation = trip.PickupLocation,
                DropoffLocation = trip.DropoffLocation,
                CreatedAt = DateTime.UtcNow,
                Status = null   
            };

            await _producer.ProduceAsync("find_car", evt.Id.ToString(), evt);

            await _responseCache.SetCacheResponseAsync("trip:waiting:", trip.UserId, evt, TimeSpan.FromMinutes(5));

            return Ok(new { message = "Đang tìm xe", trip = evt });
        }

        [HttpGet("view-trip")]
        public async Task<IActionResult> GetCache(string key)
        {
            return await _responseCache.GetCacheAsJsonAsync<Trip>("trip:waiting:", key);
        }

        [HttpDelete("remove-trip")]
        public async Task<IActionResult> RemoveCache(string key)
        {
            return Ok(await _responseCache.DeleteKeyInRedis("trip:waiting", key));
        }

        [HttpPost("push-fcm")]
        public async Task<IActionResult> PushFCMKey([FromQuery] string userId, [FromQuery] string fcmToken)
        {
            await _responseCache.SetCacheResponseAsync("fcm-token-user:", userId, fcmToken, null);
            return Ok(new { message = "Da gui thanh cong" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromQuery] string userId)
        {
            await _responseCache.DeleteKeyInRedis("fcm-token-user:", userId);
            return Ok(new { message = "Da dang xuat thanh cong" });
        }

        
    }
}
