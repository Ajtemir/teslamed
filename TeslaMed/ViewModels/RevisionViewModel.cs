using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class RevisionViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [Required(ErrorMessage = "NameRequired")]
        public string Name { get; set; }
        [Required(ErrorMessage = "UnitRequired")]
        public string Unit { get; set; }
        [Required(ErrorMessage = "ActualRemainderRequired")]
        [Range(1, int.MaxValue, ErrorMessage = "NumberError")]
        public int ActualRemainder { get; set; }
        public int SystemRemainder { get; set; }
        public int Variance { get; set; }
        [Required(ErrorMessage = "InventoryRequired")]
        public int InventoryId { get; set; }
        public int InventoryNameId { get; set; }
    }
}
