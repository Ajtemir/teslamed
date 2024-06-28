using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TeslaMed.Models
{
    public class Specialization
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "NameRequired")]
        [Display(Name = "NameDisplay")]
        public string Name { get; set; }
        public List<User>? Users { get; set; }
        
    }
}
