
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using StackExchange.Redis;
using TestKafka.Model;
using TestKafka.Redis;
using TestKafka.Service;
using static MongoDB.Driver.WriteConcern;

namespace TestKafka
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

            builder.Services.AddSingleton<MongoContext>();
            builder.Services.AddSingleton(typeof(KafkaProducerService<,>));

            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<IResponseCache, ResponseCache>();

            //Cache
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

            //builder.Services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowAllOrigins",
            //        builder => builder.AllowAnyOrigin()
            //                          .AllowAnyMethod()
            //                          .AllowAnyHeader());
            //});

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
            redis.GetDatabase().Execute("CONFIG", "SET", "notify-keyspace-events", "Ex");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            //app.UseCors("AllowAllOrigins");


            app.MapControllers();

            app.Run();
        }
    }
    

}
