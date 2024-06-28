using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TeslaMed.Models
{
    public class FooterConfiguration
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "PhoneRequired")]
        [Display(Name = "PhoneDisplay")]
        public string Phone1 { get; set; }
        [Required(ErrorMessage = "PhoneRequired")]
        [Display(Name = "PhoneDisplay")]
        public string Phone2 { get; set; }
        [Required(ErrorMessage = "PhoneRequired")]
        [Display(Name = "PhoneDisplay")]
        public string Phone3 { get; set; }
        [Required(ErrorMessage = "EmailRequired")]
        [Display(Name = "EmailDisplay")]
        public string Email { get; set; }
        [Required(ErrorMessage = "AddressRequired")]
        [Display(Name = "AddressDisplay")]
        public string Address { get; set; }
    }

}
