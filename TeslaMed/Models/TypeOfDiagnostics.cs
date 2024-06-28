using System.ComponentModel.DataAnnotations;

namespace TeslaMed.Models
{
    public class TypeOfDiagnostics
    {
        public int Id { get; set; }
      
        [Required(ErrorMessage = "NameRequired")]
        [Display(Name = "NameDisplay")]
        public string Name { get; set; }
      
        [Required(ErrorMessage = "TypeRequired")]
        [Display(Name = "TypeDisplay")]
        public int ResearchMethodId { get; set; }
        public ResearchMethod? ResearchMethod { get; set; }
      
        [Required(ErrorMessage = "PriceRequired")]
        [Range(0, int.MaxValue, ErrorMessage = "PriceRange")]
        [Display(Name = "PriceDisplay")]
        public int Price { get; set; }
        
      
        public List<Diagnostics>? Diagnostics { get; set; }
        
    }
}
