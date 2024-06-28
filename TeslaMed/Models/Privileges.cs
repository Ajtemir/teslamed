using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TeslaMed.Models
{
    public class Privileges
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "DiscountRequired")]
        [Display(Name = "DiscountDisplay")]
        public int Discount { get; set; }
        [Required(ErrorMessage = "CategoryRequired")]
        [Display(Name = "CategoryDisplay")]
        public string Category { get; set; }
        [Required(ErrorMessage = "DescriptionRequired")]
        [Display(Name = "DescriptionDisplay")]
        public string Description { get; set; }
    }
}
