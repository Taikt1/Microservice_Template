using StackExchange.Redis;

namespace TestKafka.Redis
{
    public class RedisKeyExpirationSubscriber : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisKeyExpirationSubscriber(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscriber = _redis.GetSubscriber();

            // Subscribe to expired event
            await subscriber.SubscribeAsync("__keyevent@0__:expired", async (channel, key) =>
            {
                Console.WriteLine($"[Redis Expired] Key expired: {key}");

                // Gửi thông báo FCM
                //await _firebaseService.SendNotificationToClientAsync(key);
            });

            // Không kết thúc task để giữ subscriber chạy
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
