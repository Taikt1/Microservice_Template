
using Order.App;
using Order.App.Model;
using Order.App.Services;

namespace Order.App.Event
{
    public class AcceptDriverTripEvent : BackgroundService
    {


        private readonly KafkaConsumerService<string, TripEvent> _consumer;
        private readonly TripService _tripService;
        public AcceptDriverTripEvent(KafkaConsumerService<string, TripEvent> consumer, TripService tripService)
        {
            _consumer = consumer;
            _tripService = tripService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {

                _ = Task.Run(() => _consumer.ConsumeAsync("accept_order", async (key, value) =>
                {
                    Console.WriteLine($"Received order: ID customer:{value.CustomerId} tripID:{value.Id} - {value.Price} - {value.PickupLocation} - {value.DropoffLocation} - ID driver {value.DriverId}");

                    await _tripService.Create(new Trip
                    {
                        Id = value.Id,
                        CustomerId = value.CustomerId,
                        DriverId = value.DriverId,
                        PickupLocation = value.PickupLocation,
                        DropoffLocation = value.DropoffLocation,
                        Price = value.Price,
                        StartTime = value.StartTime, // Set the start time to now
                        CreatedAt = value.CreatedAt // Set the created time to now
                    });

                }, stoppingToken), stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kafka consume error");
            }
        }
    }
}
