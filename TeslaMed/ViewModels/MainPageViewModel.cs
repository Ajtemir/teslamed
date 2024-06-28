using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class MainPageViewModel
    {
        public News Publication { get; set; }
        public List<MainDoctor> MainDoctors { get; set; }
        public string Phone1 { get; internal set; }
        public string Phone2 { get; internal set; }
        public string Phone3 { get; internal set; }
        public string Email { get; internal set; }
        public string Address { get; internal set; }
    }
}
