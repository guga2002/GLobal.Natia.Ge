using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Models.ViewModels;

public class RecieverViewModel
{
    [Display(Name = "არხის სახელი")]
    public string? CHanellName { get; set; }
    [Display(Name = "სტრიმი,სიხშირე")]
    public string? Frequency { get; set; }
    [Display(Name = "არხის ფორმატი")]
    public string? ChanellFormat { get; set; }
    [Display(Name = "იემერის მისამართი")]
    public string? EmrNumber { get; set; }
    [Display(Name = "ქარდი")]
    public string? Card { get; set; }
    [Display(Name = "პორტი")]
    public string? Port { get; set; }

    [Display(Name = "კოდირების ტიპი")]
    public string? Encryption { get; set; }
}
