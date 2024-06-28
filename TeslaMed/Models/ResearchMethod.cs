using System.ComponentModel.DataAnnotations;

namespace TeslaMed.Models
{
    public class ResearchMethod
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Пожалуйста, введите название")]
        [Display(Name = "Название способа")]
        public string Name { get; set; }
    }
}
