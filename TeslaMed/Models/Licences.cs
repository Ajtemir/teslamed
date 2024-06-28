using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TeslaMed.Models
{
    public class Licences
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "ImageRequired")]
        [Display(Name = "ImageDisplay")]
        public List<string> Photos { get; set; }
    }
}
