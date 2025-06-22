
using Driver.App.Event;
using Driver.App.Firebase;
using Driver.App.Model;
using Driver.App.Redis;
using Driver.Service;
using StackExchange.Redis;

namespace Driver.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
          
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<MongoContext>();
            builder.Services.AddSingleton(typeof(KafkaConsumerService<,>));
            builder.Services.AddSingleton(typeof(KafkaProducerService<,>));
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<IResponseCache, ResponseCache>();
            builder.Services.AddHostedService<AcceptOrderConsumer>();
            builder.Services.AddHttpClient(); // Đăng ký HttpClient
            builder.Services.AddSingleton<FirebaseNotificationService>();


            var redisConfig = builder.Configuration.GetSection("Redis");
            string redisConnectionString = redisConfig.GetValue<string>("ConnectionString");
            bool redisEnabled = redisConfig.GetValue<bool>("Enable");

            if (redisEnabled)
            {
                // Cấu hình Redis connection
                builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

                // Thêm Redis Cache nếu Redis được bật
                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "Redis:";
                });
            }

            builder.Services.AddHostedService<RedisKeyExpirationSubscriber>(); //Trigger chay background khi key het han

            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
