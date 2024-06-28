using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TeslaMed.Models
{
    public class Supply
    {
        public int Id { get; set; }
        public DateTime DateOfCreation { get; set; }
        [Required(ErrorMessage = "RequiredQuantity")]
        [Range(1, int.MaxValue, ErrorMessage = "RangeQuantity")]
        [Display(Name = "DisplayQuantity")]
        public int SupplyQuantity { get; set; }
        public int? TotalAmountWithSupply { get; set; }

        [Required(ErrorMessage = "RequiredName")]
        [Display(Name = "DisplayName")]
        public int InventoryId { get; set; }
        public Inventory? Inventory { get; set; }
    }
}
