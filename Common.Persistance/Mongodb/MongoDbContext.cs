using Common.Persistance.Entities;
using MongoDB.Driver;

namespace Common.Persistance.Mongodb
{
    public class MongoDbContext
    {
        protected MongoClient _client;
        public MongoDbContext()
        {
            _client = new MongoClient("mongodb://localhost:27017/");
            var database = _client.GetDatabase("RegionTest");
            Frequencies = database.GetCollection<RegionFrequency>("RegionFrequencies");
        }

        public IMongoCollection<RegionFrequency> Frequencies { get; set; }
    }
}
