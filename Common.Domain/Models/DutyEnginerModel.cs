using System.Web.Mvc;

namespace Common.Domain.Models
{
    public class DutyEnginerModel
    {
        public string? SelectedOption { get; set; }
        public List<SelectListItem>? OptionList { get; set; }
    }
}
