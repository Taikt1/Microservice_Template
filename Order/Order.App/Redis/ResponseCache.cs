using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Order.App.Redis
{
    public class ResponseCache : IResponseCache
    {
        private IDistributedCache _distributedCache;
        private IConnectionMultiplexer _connectionMultiplexer;

        public ResponseCache(IDistributedCache distributedCache, IConnectionMultiplexer connectionMultiplexer)
        {
            _distributedCache = distributedCache;
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<string> GetCacheResponseAsync(string cachekey)
        {
            var cacheResponse = await _distributedCache.GetStringAsync(cachekey);

            return string.IsNullOrEmpty(cacheResponse) ? null : cacheResponse;
        }

        public async Task SetCacheResponseAsync(string pattern, string cachekey, object response, TimeSpan? timeToLive)
        {
            if (response == null)
            {
                return;
            }
            //Chu thich:
            // - JsonConvert.SerializeObject: Dua ve doi tuong json quet tham chieu vong lap (Giup tranh vong lap tham chieu - cau hinh trong program.cs) (Do minh dung JsonIgnore)
            // - JsonSerializer.Serialize: Chuyển đổi dữ liệu thành chuỗi JSON dang System.Text.Json (Cho toc do nhanh hon so voi Newtonsoft.Json) 

            // Chuyển đổi dữ liệu thành chuỗi JSON, sử dụng camelCase cho các thuộc tính
            var dataToCache = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  // Sử dụng camelCase cho các thuộc tính
                WriteIndented = true,                               // Nếu bạn muốn kết quả JSON dễ đọc (có thụt lề)
                ReferenceHandler = ReferenceHandler.IgnoreCycles    // Nếu bạn muốn bỏ qua vòng lặp tham chiếu (giống như JsonIgnore)
            });

            await _distributedCache.SetStringAsync(pattern + cachekey, dataToCache, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = timeToLive
            });
        }

        public async Task<IActionResult> GetCacheAsJsonAsync<TValue>(string pattern, string key)
        {
            try
            {
                var cacheValue = await _distributedCache.GetStringAsync(pattern + key);

                if (string.IsNullOrEmpty(cacheValue))
                {
                    return new NotFoundObjectResult(new { message = $"❌ Cache not found for key: {key}" });
                }

                // Deserialize lại object từ chuỗi JSON
                var deserialized = JsonSerializer.Deserialize<TValue>(cacheValue, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return new JsonResult(deserialized)
                {
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new
                {
                    message = "Lỗi khi đọc cache",
                    exception = ex.Message
                });
            }
        }

        public List<string> GetKeyCacheListAsJson(string pattern)
        {
            try
            {
                // Lấy server Redis
                var server = _connectionMultiplexer.GetServer("localhost", 6400);
                var db = _connectionMultiplexer.GetDatabase(0);
                var keys = server.Keys(pattern: "*" + pattern + "*").Select(k => k.ToString()).ToList();


                foreach (var key in keys)
                {
                    Console.WriteLine("🧩 Found Redis key: " + key);
                }

                return keys;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteKeyInRedis(string pattern, string value)
        {
            var server = _connectionMultiplexer.GetServer("localhost", 6400);
            var db = _connectionMultiplexer.GetDatabase(0);

            //Câu lệnh tìm ra key của Redis
            //var allKeys = server.Keys(database: 0, pattern: "*").ToArray();
            //foreach (var key in allKeys)
            //{
            //    Console.WriteLine("🔑 Key hiện có: " + key);
            //}

            var fullPattern = $"Redis:{pattern}:{value}"; // hoặc "Redis:*" nếu chưa rõ
            var keys = server.Keys(database: 0, pattern: fullPattern).ToArray();

            Console.WriteLine($"Tìm thấy {keys.Length} keys để xóa.");
            foreach (var key in keys)
            {
                Console.WriteLine($"🧹 Đang xóa key: {key}");
                await db.KeyDeleteAsync(key);
            }

            return true;
        }

        public async Task<bool> DeleteKeyListInRedis(string pattern)
        {
            try
            {
                // Lấy server Redis
                var server = _connectionMultiplexer.GetServer("localhost", 6400);
                var db = _connectionMultiplexer.GetDatabase(0);
                var keys = server.Keys(pattern: "*" + pattern + "*").Select(k => k.ToString()).ToList();

                foreach (var key in keys)
                {
                    Console.WriteLine($"🧹 Đang xóa key: {key}");
                    await db.KeyDeleteAsync(key);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
}
