namespace Common.Domain.Models;

public class SatteliteFrequencyModel
{
    public int Id { get; set; }

    public string? Degree { get; set; }

    public string? Frequency { get; set; }

    public string? SymbolRate { get; set; }

    public char? Polarisation { get; set; }

    public int PortIn250 { get; set; }

    public bool HaveError { get; set; } = false;

    public int EmrNumber { get; set; }

    public int portNumber { get; set; }

    public int CardNumber { get; set; }

    public string? mer { get; set; }

    public List<ChanellModel> chanellid { get; set; } = new List<ChanellModel>();
}
