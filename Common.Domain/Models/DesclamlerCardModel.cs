namespace Common.Domain.Models;

public class DesclamlerCardModel
{
    public required string CardManufacturer { get; set; }

    public required string CardCode { get; set; }

    public Guid DesclamblerId { get; set; }

    public DesclamblerModel? desclambler { get; set; }
}
