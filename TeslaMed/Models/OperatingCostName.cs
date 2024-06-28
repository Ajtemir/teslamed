using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TeslaMed.Models
{
    public class OperatingCostName
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "NameRequired")]
        [Display(Name = "NameDisplay")]
        public string Name { get; set; }
        [Required(ErrorMessage = "UnitRequired")]
        [Display(Name = "UnitDisplay")]
        public string Unit { get; set; }
    }
}
