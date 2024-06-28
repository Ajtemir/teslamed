using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class PreEntryViewModel
    {
        public int DiagnosticsId { get; set; }
        public int SelectedPatientId { get; set; }
        public int? DoctorId { get; set; }
        [Required(ErrorMessage = "Выберите дату!")]
        [Remote("CheckDate", "Doctors", ErrorMessage = "Выберите текущую или будущую дату.")]
        public DateTime Date { get; set; }
        [Required(ErrorMessage = "Укажите время начала!")]
        [Range(0, 23, ErrorMessage = "Пожалуйста, выберите значение от 0 до 23.")]
        public int SelectedStartHour { get; set; }
        [Required(ErrorMessage = "Укажите время начала!")]
        public int SelectedStartMinute { get; set; }
        [Required(ErrorMessage = "Укажите время окончания!")]
        [Range(0, 23, ErrorMessage = "Пожалуйста, выберите значение от 0 до 23.")]
        public int SelectedEndHour { get; set; }
        [Required(ErrorMessage = "Укажите время окончания!")]
        public int SelectedEndMinute { get; set; }
        public List<TypeOfDiagnostics> TypesOfDiagnostics { get; set; } = new List<TypeOfDiagnostics>();
        [Required(ErrorMessage = "Выберите тип диагностики!")]
        public int SelectedTypeOfDiagnosticsId { get; set; }
    }
}
