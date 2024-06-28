using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.CodeAnalysis;
using System.ComponentModel.DataAnnotations;
using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class DicomFilesSaveViewModel
    {
        public int DiagnosticId { get; set; }
        public Diagnostics? Diagnostic { get; set; }
        public int TypeOfDiagnosticsId { get; set; }
        public TypeOfDiagnostics? TypeOfDiagnostics { get; set; }
        [Remote(action: "DicomCheck", controller: "Patients", ErrorMessage = "ErrorDicomCheck")]
        [Required(ErrorMessage = "DicomRequired")]
        public IFormFileCollection DicomFiles { get; set; }
    }
}
