using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
namespace TeslaMed.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "NameRequired")]
        [Display(Name = "NameDisplay")]
        public string Name { get; set; }
        [Required(ErrorMessage = "TextRequired")]
        [Display(Name = "TextDisplay")]
        public string Text { get; set; }
        [Required(ErrorMessage = "ImageRequired")]
        [Display(Name = "ImageDisplay")]
        public string Image { get; set; }
    }
}
