using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TeslaMed.Models
{
    public class PlaceOfWork
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "PlaceOfWorkRequired")]
        [Display(Name = "PlaceOfWorkDisplay")]
        public string Name { get; set; }
        public List<Doctor>? Doctors { get; set; }
    }
}
