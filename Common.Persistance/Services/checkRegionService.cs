using Common.Persistance.Entities;
using Common.Persistance.Interface;
using Common.Persistance.Mongodb;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Common.Persistance.Services;

public class checkRegionService : IcheckRegion
{
    private readonly MongoDbContext _Context;

    public checkRegionService()
    {
        _Context = new MongoDbContext();
    }

    public async Task<bool> FillDatabase(List<RegionFrequency> regions)
    {
        foreach (var item in regions)
        {
            var existingEntry = await _Context.Frequencies.Find(f =>
                f.EmrAddress == item.EmrAddress &&
                f.Card == item.Card &&
                f.port == item.port
            ).FirstOrDefaultAsync();

            if (existingEntry == null)
            {
                item.Id = ObjectId.GenerateNewId();
                await _Context.Frequencies.InsertOneAsync(item);
            }
        }

        return true;
    }

    public async Task<List<RegionResponse>> GetRegionInfo()
    {
        var response = new List<RegionResponse>();

        var groupedFrequencies = _Context.Frequencies.AsQueryable()
            .GroupBy(io => io.RegionName);

        foreach (var group in groupedFrequencies)
        {
            var regionResponse = new RegionResponse
            {
                RegionName = group.Key,
                Details = new List<Detail>()
            };

            foreach (var item in group)
            {
                var mer = await GetMer(item.Card, item.port, item.EmrAddress);

                regionResponse.Details.Add(new Detail
                {
                    Card = item.Card + 1,
                    Port = item.port + 1,
                    Mer = mer,
                    Sixshire = item.Sixshire,
                });
            }

            response.Add(regionResponse);
        }

        return response;
    }

    private async Task<string> GetMer(int card, int port, string EmrAddress)
    {
        try
        {
            var rand = new Random();
            var client = new HttpClient();
            var res = await client.GetAsync($"http://192.168.{EmrAddress}/goform/formEMR30?type=2&cmd=1&language=0&slotNo={card}&portNo={port}&ran=0.62381741{rand.Next()}");

            var ser = await res.Content.ReadAsStringAsync();

            var mer = ser.Split("<*1*>");
            if (mer.Length > 3)
            {
                return mer[3];
            }
            return "";
        }
        catch (Exception)
        {
            return "";
        }
    }
}
