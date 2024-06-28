using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TeslaMed.Models
{
    public class Link
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "TitleRequired")]
        [Display(Name = "TitleDisplay")]
        public string Title { get; set; }

        [Required(ErrorMessage = "UrlRequired")]
        [Display(Name = "UrlDisplay")]
        public string Url { get; set; }
    }
}
