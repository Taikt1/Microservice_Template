using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.App.Model;
using Order.App.Services;
using System.Threading.Tasks;

namespace Order.App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly TripService _tripService;
        private readonly KafkaProducerService<string, Trip> _producer;

        public OrderController(TripService tripService, KafkaProducerService<string, Trip> producer)
        {
            _tripService = tripService;
            _producer = producer;

        }


        [HttpPut("finish-order")]
        public async Task<IActionResult> UpdateOrder(string Id, string status)
        {
            // Logic to update an order
            var trip = _tripService.GetById(Id).Result;
            if (trip == null)
            {
                return NotFound($"Trip with ID {Id} not found.");
            }
            trip.Status = status.ToLower() == "true"; // Assuming status is a string "true" or "false"

            trip.EndTime = DateTime.UtcNow; // Set the end time to now

            await _tripService.UpdateAsync(Id, trip);

            await _producer.ProduceAsync("finish_order", Id, trip);

            return Ok();




        }

       



    }
}
