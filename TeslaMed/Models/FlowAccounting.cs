using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TeslaMed.Models
{
    public class FlowAccounting
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        [Required(ErrorMessage = "InventoryError")]
        [Display(Name = "InventoryIdName")]
        public int InventoryId { get; set; }
        public Inventory? Inventory { get; set; }
        [Required(ErrorMessage = "QuantityEmptyError")]
        [Range(1, int.MaxValue, ErrorMessage = "QuantityRangeError")]
        [Display(Name = "QuantityName")]
        public int Quantity { get; set; }
        public int InventoryTotalBalance { get; set; }
    }
}
