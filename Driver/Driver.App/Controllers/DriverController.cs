using Driver.App;
using Driver.App.Model;
using Driver.App.Redis;
using Driver.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace Notification.App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IResponseCache _responseCache;

        public DriverController(UserService userService, IResponseCache responseCache)
        {
            _userService = userService;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User request)
        {


            // Xử lý đăng ký ở 
            await _userService.CreateAsync(request);
            return Ok();
        }

        [HttpPost("push-fcm")]
        public async Task<IActionResult> PushFCMKey([FromQuery] string driverId, [FromQuery] string fcmToken)
        {
            var cachekey = $"fcm-token-driver:{driverId}";
            await _responseCache.SetCacheResponseAsync("fcm-token-driver:", driverId, fcmToken, null);
            return Ok(new { message = "Da gui thanh cong" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromQuery] string driverId)
        {
            await _responseCache.DeleteKeyInRedis("fcm-token-driver", driverId);
            return Ok(new { message = "Da dang xuat thanh cong" });
        }




    }
}
