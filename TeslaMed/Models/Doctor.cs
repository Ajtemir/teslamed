using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TeslaMed.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "NameRequired")]
        [Display(Name = "NameDisplay")]
        public string Name { get; set; }

        [Required(ErrorMessage = "SurnameRequired")]
        [Display(Name = "SurnameDisplay")]
        public string Surname { get; set; }

        [Display(Name = "PatronymicDisplay")]
        public string? Patronymic { get; set; } 
        
        public int? PlaceOfWorkId { get; set; }

        public PlaceOfWork? PlaceOfWork { get; set; }

        [Required(ErrorMessage = "PostRequired")]
        [Display(Name = "PostDisplay")]
        public string Post { get; set; }

        [Required(ErrorMessage = "AddressRequired")]
        [Display(Name = "AddressDisplay")]
        public string Address { get; set; }

        [Required(ErrorMessage = "PhoneRequired")]
        [Display(Name = "PhoneDisplay")]
        public string PhoneNumber { get; set; }

        public int? ManagerId { get; set; }

        public User? Manager { get; set; }
    }
}
