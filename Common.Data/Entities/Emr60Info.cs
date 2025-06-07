using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities;

[Table("Emr60infos")]
public class Emr60Info : AbstractEntity
{
    [Column("Reciever_port")]
    public string? Port { get; set; }
    [Column("Source_emr")]
    public int SourceEmr { get; set; }
}
