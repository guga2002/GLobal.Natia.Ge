namespace Common.Domain.Models;

public class DesclamblerModel
{
    public required int EmrNumber { get; set; }

    public required int Card { get; set; }

    public required int Port { get; set; }

    public Guid Source_ID { get; set; }

    public virtual IEnumerable<DesclamlerCardModel>? DescCard { get; set; }
}
