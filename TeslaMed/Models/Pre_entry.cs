namespace TeslaMed.Models
{
    public class Pre_entry
    {
        public int Id { get; set; }
        public int? DoctorId { get; set; }
        public User? Doctor { get; set; }
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        public int DiagnosticsId { get; set; }
        public Diagnostics? Diagnostics { get; set; }
        public int TypeOfDiagnosticsId { get; set; }
        public TypeOfDiagnostics? TypeOfDiagnostics { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
