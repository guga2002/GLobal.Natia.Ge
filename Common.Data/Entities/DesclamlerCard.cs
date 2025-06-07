using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities;

[Table("DesclamlerCards")]
public class DesclamlerCard : AbstractEntity
{
    [Column("Card_Manufacturer")]
    public string? CardManufacturer { get; set; }

    [Column("Card_Code")]
    public string? CardCode { get; set; }

    [ForeignKey("desclambler")]
    public int DesclamblerId { get; set; }

    public Desclambler? desclambler { get; set; }
}
