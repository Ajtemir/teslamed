using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using TeslaMed.Models;
namespace TeslaMed.Models
{
    public class Revision
    {
        public int Id { get; set; }
        [Display(Name = "DateDisplay")]
        public DateTime Date { get; set; }

        [Display(Name = "NameDisplay")]
        public string Name { get; set; }

        [Display(Name = "UnitDisplay")]
        public string Unit { get; set; }

        [Display(Name = "ActualRemainderDisplay")]
        [Range(1, int.MaxValue, ErrorMessage = "NumberError")]
        public int ActualRemainder { get; set; }

        [Display(Name = "SystemRemainder")]
        public int SystemRemainder { get; set; }

        [Display(Name = "Variance")]
        public int Variance { get; set; }
        public int InventoryId { get; set; }
        public int InventoryNameId { get; set; }
        public InventoryName? InventoryName {get; set;}
        public Inventory? Inventory { get; set; }

        public Revision()
        {
            Date = DateTime.UtcNow;
        }
    }
}
