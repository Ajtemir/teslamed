using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeslaMed.Models
{
    public class MainDoctor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "NameRequired")]
        [Display(Name = "NameDisplay")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "DescriptionRequired")]
        [Display(Name = "DescriptionDisplay")]
        public string Description { get; set; }

        [Required(ErrorMessage = "ImageRequired")]
        [Display(Name = "ImageDisplay")]
        public string Image { get; set; }
    }
}
