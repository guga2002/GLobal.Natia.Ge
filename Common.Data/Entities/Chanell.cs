using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities;


[Table("Chanells")]
public class Chanell : AbstractEntity
{
    [Column("Name_Of_Chanell")]
    public string? Name { get; set; }
    public List<Source> Sources { get; set; } = new List<Source>();
}
