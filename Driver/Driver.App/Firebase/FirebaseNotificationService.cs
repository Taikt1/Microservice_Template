using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Driver.App.Firebase
{

    public class FirebaseNotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public FirebaseNotificationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string> data = null)
        {
            var serverKey = _configuration["Firebase:ServerKey"];
            var message = new
            {
                to = fcmToken,
                notification = new
                {
                    title = title,
                    body = body
                },
                data = data
            };

            var jsonMessage = JsonConvert.SerializeObject(message);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send");
            request.Headers.TryAddWithoutValidation("Authorization", $"key={serverKey}");
            request.Content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📤 FCM sent. Status: {response.StatusCode}, Response: {responseContent}");
        }
    }

}
