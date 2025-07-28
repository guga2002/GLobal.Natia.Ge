using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Models.ViewModels;

public class TranscoderViewModel
{

    [Display(Name = "არხის სახელი")]
    public string? Id { get; set; }

    [Display(Name = "იემერის მისამართი")]
    public string? EmrNumber { get; set; }

    [Display(Name = "ქარდი")]
    public string? Card { get; set; }

    [Display(Name = "პორტი")]
    public string? Port { get; set; }

    [Display(Name = "ტრანსკოდირების ფორმატი")]
    public string? TranscodingFormat { get; set; }
}
