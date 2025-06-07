using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities;

[Table("Users")]
[Microsoft.EntityFrameworkCore.Index(nameof(PersonalNumber), IsUnique = true)]
public class User : IdentityUser
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
