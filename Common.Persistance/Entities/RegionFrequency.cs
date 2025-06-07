using MongoDB.Bson;

namespace Common.Persistance.Entities;

public class RegionFrequency
{
    public ObjectId Id { get; set; }

    public string? RegionName { get; set; }

    public string? EmrAddress { get; set; }

    public int Card { get; set; }

    public int port { get; set; }

    public int Sixshire { get; set; }
}
