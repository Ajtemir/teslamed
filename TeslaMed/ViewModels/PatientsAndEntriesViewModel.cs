using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class PatientsAndEntriesViewModel
    {
        public List<Pre_entry> Entries { get; set; }
        public List<Patient> Patients { get; set; }
    }
}
