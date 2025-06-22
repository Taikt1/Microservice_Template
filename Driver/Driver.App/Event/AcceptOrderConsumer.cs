using Driver.App.Model;
using Driver.App.Redis;
using Driver.Service;
using MongoDB.Bson.IO;
using System.Xml;
using Newtonsoft.Json;
using Driver.App.Firebase;

namespace Driver.App.Event
{
    public class AcceptOrderConsumer : BackgroundService
    {
        private readonly KafkaConsumerService<string, TripEvent> _consumer;
        private readonly ILogger<AcceptOrderConsumer> _logger;
        private readonly UserService _userService;
        private readonly KafkaProducerService<string, TripEvent> _producer;
        private readonly IResponseCache _responseCache;
        private readonly FirebaseNotificationService _firebaseService;

        public AcceptOrderConsumer(KafkaConsumerService<string, TripEvent> consumer, ILogger<AcceptOrderConsumer> logger, UserService userService, KafkaProducerService<string, TripEvent> producer, IResponseCache responseCache, FirebaseNotificationService firebaseService)
        {
            _consumer = consumer;
            _logger = logger;
            _userService = userService;
            _producer = producer;
            _responseCache = responseCache;
            _firebaseService = firebaseService;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
                try
                {
                _ = Task.Run(() => _consumer.ConsumeAsync("find_car", async (key, value) =>
                {
                    Console.WriteLine($"Received order: ID customer:{value.CustomerId} tripID:{value.Id} - {value.Price} - {value.PickupLocation} - {value.DropoffLocation} - ID driver {value.DriverId}");


                    //Lay driver dang dang nhap
                    var list = _responseCache.GetKeyCacheListAsJson("fcm-token-driver");
                    var driver = await GetDriverLogin(list);

                    Console.WriteLine($"📦 Kafka value received:\n{value}");
                    // Ghi vào Redis(5 phút)
                    foreach (var user in driver)
                    {
                        Console.WriteLine(user.Id);

                        //Luu gia tri tung driver vao cache
                        var cacheKey = $"{value.Id}:{user.Id}";
                        await _responseCache.SetCacheResponseAsync("trip:waiting:", cacheKey, value, TimeSpan.FromMinutes(5));


                        var getValue = await _responseCache.GetCacheAsJsonAsync<string>("fcm-token-driver:", user.Id);
                        //Gui gia tri len server firebase
                        await _firebaseService.SendNotificationAsync(
                            fcmToken: $"{getValue}",
                            title: "🚖 Có chuyến mới",
                            body: "Có khách đang cần đặt xe gần bạn!",
                            data: new Dictionary<string, string>
                            {
                                { "tripId", value.Id },
                                { "pickupLocation", value.PickupLocation },
                                { "dropoffLocation", value.DropoffLocation },
                                { "price", value.Price.ToString() },
                            }
                        );
                    }

                }, stoppingToken), stoppingToken);

                await Task.Delay(1000, stoppingToken); // Giữ cho service không bị dừng
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error while starting the consumer");
                }
        }
        

        private async Task<List<User>> GetDriverLogin(List<string> list)
        {
            var driverlist = new List<User>();
            foreach (var driver in list)
            {
                string[] value = driver.Split(':');
                var user = await _userService.GetUserAsync(value[value.Length - 1]);
                if (driver != null)
                {
                    driverlist.Add(user);
                }
            }
            return driverlist;
        }
    }
}


