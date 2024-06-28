using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class PatientDetailsViewModel
    { 
        public Patient Patient { get; set; }
        public List<Diagnostics>? Diagnostics { get; set; }
        public List<TypeOfCashlessPayment> TypesOfCashlessPayment { get; set; }
    }
}
