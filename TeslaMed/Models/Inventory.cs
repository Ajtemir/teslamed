using System.ComponentModel.DataAnnotations;

namespace TeslaMed.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "TotalAmountRequired")]
        [Display(Name = "TotalAmountDisplay")]
        public int TotalAmount { get; set; }
        public DateTime DateOfCreation { get; set; }

        [Required(ErrorMessage = "NameRequired")]
        [Display(Name = "NameDisplay")]
        public int InventoryNameId { get; set; }
        public InventoryName? InventoryName { get; set; }
    }
}
