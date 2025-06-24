using Confluent.Kafka;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Text.Json;
using System.Threading.Tasks;
using static MongoDB.Driver.WriteConcern;

namespace Order.App;
public class KafkaProducerService<TKey, TValue>
{

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