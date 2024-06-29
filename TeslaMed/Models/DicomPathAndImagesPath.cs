using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeslaMed.Models
{
    public class DicomPathAndImagesPath
    {
        public int Id { get; set; }
        public List<string> DicomPaths { get; set; }
        public List<string> ImagePaths { get; set; }
        public int? Rating { get; set; }
        public string? Conclusion { get; set; }
        public int TypeOfDiagnosticsId { get; set; }
        public int DiagnosticsId { get; set; }
        [ForeignKey(nameof(DiagnosticsId))]
        public Diagnostics Diagnostics { get; set; } = null!;
        public TypeOfDiagnostics? TypeOfDiagnostics { get; set; }
        public bool IsDicomDownload { get; set; }
    }
}
