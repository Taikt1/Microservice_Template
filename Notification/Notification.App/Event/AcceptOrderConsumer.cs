using MongoDB.Bson.IO;
using System.Xml;
using Newtonsoft.Json;
using Driver.App.Firebase;
using FirebaseAdmin.Messaging;
using Notification.App.Redis;

namespace Notification.App.Event
{
    public class AcceptOrderConsumer : BackgroundService
    {
        private readonly KafkaConsumerService<string, Trip> _consumer;
        private readonly KafkaConsumerService<string, Trip> _consumer1;
        private readonly ILogger<AcceptOrderConsumer> _logger;
        private readonly KafkaProducerService<string, string> _producer;
        private readonly IResponseCache _responseCache;
        private readonly FirebaseNotificationService _firebaseService;

        public AcceptOrderConsumer(KafkaConsumerService<string, Trip> consumer, ILogger<AcceptOrderConsumer> logger, KafkaProducerService<string, string> producer, IResponseCache responseCache, FirebaseNotificationService firebaseService, KafkaConsumerService<string, Trip> consumer1)
        {
            _consumer = consumer;
            _logger = logger;
            _producer = producer;
            _responseCache = responseCache;
            _firebaseService = firebaseService;
            _consumer1 = consumer1;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
                try
                {
                _ = Task.Run(() => _consumer.ConsumeAsync("find_car", async (key, value) =>
                {
                    Console.WriteLine($"Received order: ID customer:{value.CustomerId} - tripID:{value.Id} - {value.Price} - {value.PickupLocation} - {value.DropoffLocation} - ID driver {value.DriverId}");

                    //Luu chuyen di vao redis
                    var list = _responseCache.GetKeyCacheListAsJson("fcm-token-driver");
                    var driver = await GetDriverLogin(list);
                    // Ghi vào Redis(5 phút)
                    foreach (var user in driver)
                    {

                        //Luu gia tri tung driver vao cache
                        var cacheKey = $"{value.Id}:{user}";
                        await _responseCache.SetCacheResponseAsync("trip:waiting:", cacheKey, value, TimeSpan.FromMinutes(5));


                        var getValue = await _responseCache.GetCacheAsJsonAsync<string>("fcm-token-driver:", user);

                        var message = new Message()
                        {
                            Token = getValue,
                            Notification = new FirebaseAdmin.Messaging.Notification
                            {
                                Title = "Have a car",
                                Body = $"ID customer={value.CustomerId} - tripID={value.Id} - Diemdon={value.PickupLocation} - DiemDen={value.DropoffLocation} - Price={value.Price}"
                            }
                        };

                        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                    }
                }, stoppingToken), stoppingToken);

                _ = Task.Run(() => _consumer1.ConsumeAsync("finish_trip", async (key, value) =>
                {
                    var getValue = await _responseCache.GetCacheAsJsonAsync<string>("fcm-token-user:", value.CustomerId.ToString());

                    var message = new Message()
                    {
                        Token = getValue,
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = "Finish trip",
                            Body = $"ID customer={value.CustomerId} - tripID={value.Id} - Diemdon={value.PickupLocation} - DiemDen={value.DropoffLocation} - Status={value.Status} - StarTime={value.StartTime} - EndTime={value.EndTime}"
                        }
                    };

                    string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

                }, stoppingToken
                ), stoppingToken);

                _ = Task.Run(() => _consumer1.ConsumeAsync("payment_finish", async (key, value) =>
                    {
                        var getValue = await _responseCache.GetCacheAsJsonAsync<string>("fcm-token-driver:", value.DriverId.ToString());

                        var message = new Message()
                        {
                            Token = getValue,
                            Notification = new FirebaseAdmin.Messaging.Notification
                            {
                                Title = "Finish your booking",
                                Body = $"ID customer={value.CustomerId} - tripID={value.Id} - Diemdon={value.PickupLocation} - DiemDen={value.DropoffLocation} - Status={value.Status} - StarTime={value.StartTime} - EndTime={value.EndTime}"
                            }
                        };

                        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);


                    },stoppingToken
                ), stoppingToken);

                await Task.Delay(1000, stoppingToken); // Giữ cho service không bị dừng
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while starting the consumer");
                }
        }
        private async Task<List<string>> GetDriverLogin(List<string> list)
        {
            var driverlist = new List<string>();
            foreach (var driver in list)
            {
                string[] value = driver.Split(':');
                if (driver != null)
                {
                    driverlist.Add(value[value.Length - 1]);
                }
            }
            return driverlist;
        }
    }
}


