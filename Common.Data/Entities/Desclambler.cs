using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities;

[Table("Desclamlers")]
public class Desclambler : AbstractEntity
{
    [Column("Emr_Number")]
    public int EmrNumber { get; set; }

    [Column("Card_In_Desclambler")]

    public int Card { get; set; }

    [Column("Port_In_Desclambler")]
    public int Port { get; set; }

    [Column("Source_Id")]
    [ForeignKey("Source")]
    public int Source_ID { get; set; }

    public Source? Source { get; set; }

    public virtual IEnumerable<DesclamlerCard>? DescCard { get; set; }
}
