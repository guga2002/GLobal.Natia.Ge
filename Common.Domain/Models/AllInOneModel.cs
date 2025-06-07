using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Models;

public class AllInOneModel
{
    [Display(Name = "არხის სახელი")]
    public string? ChanellName { get; set; }

    [Display(Name = "ფორმატი")]
    public string? ChanellFormat { get; set; }

    [Display(Name = "აქტიურია?")]
    public bool SourceIsActive { get; set; }

    [Display(Name = "სიხშირე, სტრიმი")]
    public string? Frequency { get; set; }

    [Display(Name = "ტრანსკოდერი")]
    public string? TranscoderInfo { get; set; }

    [Display(Name = "დესკრამბლერი")]
    public string? DesclamlerInfo { get; set; }
}
