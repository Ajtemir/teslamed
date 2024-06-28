using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TeslaMed.Models
{
    public class OperatingCost
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "TotalAmountRequired")]
        [Range(1, int.MaxValue, ErrorMessage = "AmountRangeError")]
        [Display(Name = "TotalAmountDisplay")]
        public int TotalAmount { get; set; }
        public DateTime DateOfCreation { get; set; }

        [Required(ErrorMessage = "OperatingCostNameRequired")]
        [Display(Name = "OperatingCostNameDisplay")]
        public int OperatingCostNameId { get; set; }
        public OperatingCostName? OperatingCostName { get; set; }

        [Required(ErrorMessage = "TypeOfCostsRequired")]
        [Display(Name = "TypeOfCostsDisplay")]
        public int TypeOfCostsId { get; set; }
        public TypeOfCosts? TypeOfCosts { get; set; }
        public bool IsManagersCost { get; set; }
    }
}
