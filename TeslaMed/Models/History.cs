using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TeslaMed.Models
{
    public class History
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "TextRequired")]
        [Display(Name = "TextDisplay")]
        public string Text { get; set; }
        [Required(ErrorMessage = "ImageRequired")]
        [Display(Name = "ImageDisplay")]
        public string Image { get; set; }
    }
}
