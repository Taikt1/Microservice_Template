using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Order.App.Model;

namespace Order.App.Services
{
    public class TripService
    {
        private readonly MongoContext _mongoService;

        public TripService(MongoContext mongoService)
        {
            _mongoService = mongoService;
        }

        public async Task Create(Trip value) =>
           await _mongoService.Trips.InsertOneAsync(value);

        public async Task<Trip?> GetById(string id)
        {
            return await _mongoService.Trips.Find(tp => tp.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(string id, Trip trip)
            => await _mongoService.Trips.ReplaceOneAsync(
                tp => tp.Id == id, trip);

    }
}
