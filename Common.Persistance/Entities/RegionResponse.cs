namespace Common.Persistance.Entities;

public class RegionResponse
{
    public List<Detail>? Details { get; set; }

    public string? RegionName { get; set; }
}

public class Detail
{
    public int Card { get; set; }

    public int Port { get; set; }

    public string? Mer { get; set; }

    public int Sixshire { get; set; }
}
