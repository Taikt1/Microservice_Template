using Microsoft.AspNetCore.Mvc;

namespace Driver.App.Redis
{
    public interface IResponseCache
    {
        //Gan key
        public Task SetCacheResponseAsync(string pattern, string cachekey, object response, TimeSpan? timeToLive);

        //Lay chuoi
        public Task<string> GetCacheResponseAsync(string cachekey);

        //Lay list key
        public List<string> GetKeyCacheListAsJson(string pattern);

        //Lay key ve doi tuong
        public Task<IActionResult> GetCacheAsJsonAsync<TValue>(string pattern, string key);

        //Xoa key don
        public Task<bool> DeleteKeyInRedis(string pattern, string key);

        //Xoa list key
        public Task<bool> DeleteKeyListInRedis(string pattern);
    }
}
