using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities;

[Table("Users")]
public class User
{
    [Column("PersonalNumber")]
    public string? PersonalNumber { get; set; }
    [Required]
    [Column("name")]
    public string? Name { get; set; }
    [Column("Surname")]
    public string? Surname { get; set; }

    [Column("Birthdate")]
    public DateTime Birthdate { get; set; }

}
