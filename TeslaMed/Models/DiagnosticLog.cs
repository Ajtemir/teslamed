namespace TeslaMed.Models
{
    public class DiagnosticLog
    {
        public int Id { get; set; }
        public DateTime? ResearchStart { get; set; }
        public DateTime? ResearchEnd { get; set; }
        public List<string>? LogList { get; set; }
    }
}
