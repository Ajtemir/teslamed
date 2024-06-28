using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.ComponentModel.DataAnnotations;

namespace TeslaMed.Models
{
    public class Discount
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "NameRequired")]
        [Display(Name = "NameDisplay")]
        public string Name { get; set; }
        [Required(ErrorMessage = "PercentRequired")]
        [Display(Name = "PercentDisplay")]
        [Range(0, 100, ErrorMessage = "PercentRange")]
        public int Percent { get; set; }
    }
}
