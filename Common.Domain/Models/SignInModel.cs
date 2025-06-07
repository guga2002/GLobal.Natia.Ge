using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Models;

public class SignInModel
{
    [StringLength(20, ErrorMessage = " the field Username is bad", MinimumLength = 7)]
    [Required(ErrorMessage = "Username Is Required")]
    public string? Username { get; set; }

    [StringLength(20, ErrorMessage = " the field password is bad", MinimumLength = 7)]
    [Required(ErrorMessage = "Password Is Required")]
    public string? Password { get; set; }
}
