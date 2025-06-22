using Confluent.Kafka;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System;
using System.Text.Json;
using System.Threading;
using static MongoDB.Driver.WriteConcern;

namespace Driver.App
{
    public class KafkaConsumerService<TKey, TValue>
    {

        private readonly ConsumerConfig _config;

        public KafkaConsumerService(IConfiguration configuration)
        {
            _config = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = "DriverId",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
        }

        public async Task ConsumeAsync(string topic, Func<TKey, TValue, Task> handler, CancellationToken cancellationToken)
        {
            using var consumer = new ConsumerBuilder<TKey, string>(_config).Build();
            consumer.Subscribe(topic);

            
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = consumer.Consume(TimeSpan.FromSeconds(1)); // tránh blocking vô thời hạn

                        if (cr != null)
                        {
                            var value = JsonSerializer.Deserialize<TValue>(cr.Message.Value);
                            await handler(cr.Message.Key, value);
                        }
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"❌ Kafka consume error: {e.Error.Reason}");
                    }
                   
                }
          
        }
    }
}
//Option 1
//public void ConsumeUserProfile()
//{
//    var config = new ConsumerConfig
//    {
//        BootstrapServers = "localhost:9092",
//        GroupId = "user-group1",
//        AutoOffsetReset = AutoOffsetReset.Earliest
//    };

//    using var consumer = new ConsumerBuilder<Null, string>(config).Build();
//    consumer.Subscribe("user-profile-topic");

//    while (true)
//    {
//        var result = consumer.Consume(CancellationToken.None);
//        var userMessage = JsonSerializer.Deserialize<Option>(result.Message.Value);

//        Console.WriteLine($"C# TestMongo Received message: {userMessage}");

//        if (userMessage.Action == "tai" )
//        {
//            Console.WriteLine($"C# TestMongo Received option 1");
//        }
//        else Console.WriteLine($"C# TestMongo Received option 2");


//    }
//}
