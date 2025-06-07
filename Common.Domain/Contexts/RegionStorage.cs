namespace Common.Domain.Contexts;

public class RegionStorage
{
    public List<string> Keys { get; set; }

    public List<string> Qutaisi { get; set; }

    public List<string> Telavi { get; set; }

    public List<string> Foti { get; set; }

    public List<string> Gori { get; set; }

    public List<string> Ports { get; set; } = new List<string>()
    {
        "Port 11","Port 12","Port 13","Port 14","Port 15","Port 16"
    };

    public RegionStorage()
    {
        Qutaisi = new List<string>();
        Telavi = new List<string>();
        Foti = new List<string>();
        Gori = new List<string>();
        Keys = new List<string>()
                {
                     "20.10",
                     "20.20",
                     "20.30",
                     "20.40",
                     "20.50",
                     "20.60",
                     "20.70",
                     "20.80",
                     "20.90",
                     "20.100",
                     "20.110",
                     "20.120",
                     "20.130",
                     "20.160",
                     "20.170",
                     "20.200",
                     "20.210",
                     "20.220",
                     "20.230",
                     "20.240",
                     "20.250",
                     "13.10",
                     "13.20",
                     "14.10",
                     "14.20",
                     "15.10",
                     "15.20",
                     "15.30",
                     "25.10",
                     "25.20",
                };
    }
}
