
namespace Notification.App.Event
{
    public class OrderDriverListEvent : BackgroundService
    {


        private readonly KafkaConsumerService<string, TripEvent> _consumer;
        public OrderDriverListEvent(KafkaConsumerService<string, TripEvent> consumer)
        {
            _consumer = consumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _ = Task.Run(() => _consumer.ConsumeAsync("find_car", async (key, value) =>
                {
                    Console.WriteLine($"Received order: ID customer:{value.CustomerId} tripID:{value.Id} - {value.Price} - {value.PickupLocation} - {value.DropoffLocation} - ID driver {value.DriverId}");

                }, stoppingToken), stoppingToken);

                _ = Task.Run(() => _consumer.ConsumeAsync("accept-trip", async (key, value) =>
                {
                    Console.WriteLine($"Received order: ID customer:{value.CustomerId} tripID:{value.Id} - {value.Price} - {value.PickupLocation} - {value.DropoffLocation} - ID driver {value.DriverId}");

                }, stoppingToken), stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Kafka consume error");
            }
        }
    }
}
