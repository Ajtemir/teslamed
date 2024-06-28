using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class PatientWithDiagnosticsViewModel
    {
        public Diagnostics Diagnostics { get; set; }
        public Patient Patient { get; set; }
        public ResearchMethod ResearchMethod { get; set; }
        public string Base64Image { get; set; }
        public int FinalSum { get; set; }
        public int Discount { get; set; }
    }
}