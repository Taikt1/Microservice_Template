using Confluent.Kafka;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Text.Json;
using System.Threading.Tasks;
using static MongoDB.Driver.WriteConcern;

namespace Notification.App;
public class KafkaProducerService<TKey, TValue>
{
    //Option 1
    //private readonly IProducer<Null, string> _producer;

    //public KafkaProducerService()
    //{
    //    var config = new ProducerConfig { BootstrapServers = "localhost:9092" };
    //    _producer = new ProducerBuilder<Null, string>(config).Build();
    //}

    //public async Task SendUserProfile(Option option)
    //{
    //    var message = JsonSerializer.Serialize(option);
    //    await _producer.ProduceAsync("user-profile-topic", new Message<Null, string> { Value = message });
    //}

    //Option 2

    private readonly IProducer<TKey, string> _producer;
    private readonly string _bootstrapServers;

    public KafkaProducerService(IConfiguration config)
    {
        _bootstrapServers = config["Kafka:BootstrapServers"];
        var producerConfig = new ProducerConfig { BootstrapServers = _bootstrapServers };
        _producer = new ProducerBuilder<TKey, string>(producerConfig).Build();
    }

    public async Task ProduceAsync(string topic, TKey key, TValue value)
    {
        var message = new Message<TKey, string>
        {
            Key = key,
            Value = JsonSerializer.Serialize(value)
        };

        await _producer.ProduceAsync(topic, message);
    }
}