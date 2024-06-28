using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class FreelanceAndStaffDoctorsListsViewModel
    {
        public List<User> StaffDoctors { get; set; }
        public List<Doctor> FreelanceDoctors { get; set; }
    }
}
