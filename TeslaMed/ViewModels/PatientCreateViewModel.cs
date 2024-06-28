using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TeslaMed.ViewModels
{
    public class PatientCreateViewModel
    {
        [Required(ErrorMessage = "NameRequired")]
        public string Name { get; set; }
        [Required(ErrorMessage = "SurnameRequired")]
        public string Surname { get; set; }
        public string? Patronymic { get; set; }
        [Required(ErrorMessage = "GenderRequired")]
        public string Gender { get; set; }
        [Required(ErrorMessage = "BirthDateRequired")]
        [Remote(action: "BirthDateCheck",controller: "Patients", ErrorMessage = "BirthDateRemote")]
        public DateTime BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
        public string? SecondPhoneNumber { get; set; }
        public string? Comment { get; set; }
        public bool CreateDiagnostics { get; set; }

    }
}
