using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Notification.App;
using Notification.App.Event;
using Notification.App.Redis;

namespace Driver.App.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {

        private readonly KafkaProducerService<string, Trip> _producer;
        private readonly IResponseCache _responseCache;

        public NotificationController(KafkaProducerService<string, Trip> producer, IResponseCache responseCache)
        {
            _producer = producer;
            _responseCache = responseCache;
        }

        

        [HttpDelete("accept-booking")]
        public async Task<IActionResult> RemoveCache(string keyTripID, string keyDriverID)
        {
            //Xoa cac key lien quan de dua noti vao db
            var driverTrip = await _responseCache.GetCacheAsJsonAsync<Trip>("trip:waiting:", $"{keyTripID}:{keyDriverID}");

            driverTrip.DriverId = keyDriverID;
            driverTrip.StartTime = DateTime.UtcNow;

            ////Gui thong bao driver chap nhan chuyen di cho User
            //var driver = await _responseCache.GetCacheAsJsonAsync<string>("fcm-token-driver:", keyDriverID);

            //var message = new Message()
            //{
            //    Token = driver,
            //    Notification = new FirebaseAdmin.Messaging.Notification
            //    {
            //        Title = "Have a car",
            //        Body = $"ID customer={driverTrip.CustomerId} - tripID={driverTrip.Id} - Diemdon={driverTrip.PickupLocation} - DiemDen={driverTrip.DropoffLocation} - Driver ID: {keyDriverID}"
            //    }
            //};

            //string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);


            //Gui thong bao driver chap nhan chuyen di cho User
            var user = await _responseCache.GetCacheAsJsonAsync<string>("fcm-token-user:", driverTrip.CustomerId);

            var message = new Message()
            {
                Token = user,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = "Driver accept your booking",
                    Body = $"ID customer={driverTrip.CustomerId} - tripID={driverTrip.Id} - Diemdon={driverTrip.PickupLocation} - DiemDen={driverTrip.DropoffLocation} - Driver ID: {keyDriverID}"
                }
            };

            string response1 = await FirebaseMessaging.DefaultInstance.SendAsync(message);


            await _producer.ProduceAsync("accept_order", keyTripID, driverTrip);

            return Ok(await _responseCache.DeleteKeyListInRedis(keyTripID));
        }



       
    }
}
