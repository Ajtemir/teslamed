
using System.ComponentModel.DataAnnotations;

namespace TeslaMed.Models
{
    public class ArrivalType
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "NameRequired")]
        [Display(Name = "NameDisplay")]
        public string Name { get; set; }
    }
}
