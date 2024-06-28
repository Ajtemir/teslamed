using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace TeslaMed.Models
{
    public class News
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "TitleRequired")]
        [Display(Name = "TitleDisplay")]
        public string Title { get; set; }
        [Required(ErrorMessage = "DescriptionRequired")]
        [Display(Name = "DescriptionDisplay")]
        public string Description { get; set; }
        [Required(ErrorMessage = "ImageRequired")]
        [Display(Name = "ImageDisplay")]
        public string Image { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("NewsId")]
        public List<Link>? Links { get; set; } = new List<Link>();
        public DateTime Date { get; set; }
        public DateTime? EditDate { get; set; }
        public News()
        {
            Date = DateTime.Now;
        }
    }
}
