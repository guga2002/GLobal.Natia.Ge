namespace Common.Domain.Models.Region;

public class DetailViewModel
{
    public int card { get; set; }
    public int port { get; set; }
    public string? mer { get; set; }
    public int sixshire { get; set; }
}

public class RegionResponseViewModel
{
    public string? regionName { get; set; }
    public List<DetailViewModel>? details { get; set; }
}
