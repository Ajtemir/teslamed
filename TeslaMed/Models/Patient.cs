using Microsoft.AspNetCore.Identity;

namespace TeslaMed.Models
{
    public class Patient : IdentityUser<int>
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? Patronymic { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string? SecondPhoneNumber { get; set; }
        public string? Comment { get; set; }
        public string Code { get; set; }
        public bool HasDiagnostics { get; set; }
        public DateTime CreationDate { get; set; }
        public Patient()
        {
            CreationDate = DateTime.UtcNow;
            HasDiagnostics = false;
        }
    }
}
