using Driver.App.Event;
using Driver.App.Redis;
using Driver.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace Driver.App.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {

        private readonly UserService _userService;
        private readonly KafkaProducerService<string, TripEvent> _producer;
        private readonly IResponseCache _responseCache;

        public DriverController(UserService userService, KafkaProducerService<string, TripEvent> producer, IResponseCache responseCache)
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
            if (login != null)
            {
                return Ok(new { status = login });
            }
            return BadRequest("Login Failed");
        }

        [HttpPost("push-fcm")]
        public async Task<IActionResult> PushFCMKey([FromQuery] string driverId, [FromQuery] string fcmToken)
        {
            var cachekey = $"fcm-token-driver:{driverId}";
            await _responseCache.SetCacheResponseAsync("fcm-token-driver:", driverId, fcmToken, null);
            return Ok(new { message = "Da gui thanh cong"});
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromQuery] string driverId)
        {
            await _responseCache.DeleteKeyInRedis("fcm-token-driver:", driverId);
            return Ok(new { message = "Da dang xuat thanh cong" });
        }

        [HttpDelete("accept-booking")]
        public async Task<IActionResult> RemoveCache(string key)
        {
            //Xoa cac key lien quan de dua noti vao db

            return Ok(await _responseCache.DeleteKeyListInRedis(key));
        }

       
    }
}
