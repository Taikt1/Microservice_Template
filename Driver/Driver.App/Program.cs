
using Driver.App;
using Driver.App.Model;
using Driver.App.Redis;
using Driver.Service;
using StackExchange.Redis;

namespace Notification.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            builder.Services.AddSingleton(typeof(KafkaConsumerService<,>));
            builder.Services.AddSingleton(typeof(KafkaProducerService<,>));
            builder.Services.AddSingleton<IResponseCache, ResponseCache>();
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<MongoContext>();

            var redisConfig = builder.Configuration.GetSection("Redis");
            string redisConnectionString = redisConfig.GetValue<string>("ConnectionString");
            bool redisEnabled = redisConfig.GetValue<bool>("Enable");

            if (redisEnabled)
            {
                // config Redis connection
                builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

                // add Redis Cache if Redis is turn on
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
