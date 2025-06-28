using FirebaseAdmin.Messaging;
using StackExchange.Redis;

namespace Notification.App.Redis
{
    public class RedisKeyExpirationSubscriber : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IResponseCache _responseCache;

        public RedisKeyExpirationSubscriber(IConnectionMultiplexer redis, IResponseCache responseCache)
        {
            _redis = redis;
            _responseCache = responseCache;
        }

        private List<string> GetDriverLogin(List<string> list)
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscriber = _redis.GetSubscriber();

            // Subscribe to expired event
            await subscriber.SubscribeAsync("__keyevent@0__:expired", async (channel, key) =>
            {
                Console.WriteLine($"[Redis Expired] Key expired: {key}");

                // Gửi thông báo FCM
                var keySplit = key.ToString().Split(':');

                var fcmToken = await _responseCache.GetCacheAsJsonAsync<string>("fcm-token-user:", keySplit[keySplit.Length - 1]);

                if (fcmToken != null)
                {
                    var messageRemove = new Message()
                    {
                        Token = fcmToken,
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = "Booking is fail",
                            Body = $"tripID={keySplit[keySplit.Length - 2]}"
                        }
                    };
                    string response = await FirebaseMessaging.DefaultInstance.SendAsync(messageRemove);
                }
            });

            // Không kết thúc task để giữ subscriber chạy
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }

}
