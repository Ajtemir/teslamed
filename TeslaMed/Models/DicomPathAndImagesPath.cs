using System.ComponentModel.DataAnnotations;

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
        public TypeOfDiagnostics? TypeOfDiagnostics { get; set; }
        public bool IsDicomDownload { get; set; }
    }
}
