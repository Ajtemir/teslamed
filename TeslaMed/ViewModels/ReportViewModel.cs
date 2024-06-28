namespace TeslaMed.ViewModels
{
    public class ReportViewModel
    {
        public int TypeOfDiagnosticId { get; set; }
        public int DiagnosticId { get; set; }
        public int? Rating { get; set; }
        public string? Conclusion { get; set; }
    }
}
