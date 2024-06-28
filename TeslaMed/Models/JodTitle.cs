using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TeslaMed.Models
{
    public class JodTitle
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "NameRequired")]
        [Display(Name = "NameDisplay")]
        public string Title { get; set; }
    }
}
