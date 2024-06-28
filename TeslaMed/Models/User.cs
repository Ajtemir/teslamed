using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TeslaMed.Models
{
    public class User : IdentityUser<int>
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? Patronymic { get; set; }
        public string Address { get; set; }
        public virtual List<Department>? Departments { get; set; }
        public JodTitle? Post { get; set; }
        public int PostId { get; set; }
        public virtual List<Specialization>? Specializations { get; set; }
        public double? Evaluation { get; set; }
        public bool Active { get; set; }
        
    }
}
