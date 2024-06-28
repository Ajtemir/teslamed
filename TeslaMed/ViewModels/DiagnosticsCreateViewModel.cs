using System.ComponentModel.DataAnnotations;
using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class DiagnosticsCreateViewModel
    {
        public Patient? Patient { get; set; }
        public int PatientId { get; set; }
        [Required(ErrorMessage = "ArrivalTypeRequired")]
        public int ArrivalTypeId { get; set; }
        public int? ArrivalTypeDoctorId { get; set; }
        public List<Doctor>? ArrivalTypeDoctors { get; set; }
        public List<ArrivalType>? ArrivalTypes { get; set; }

        [Required(ErrorMessage = "DepartmentRequired")]
        public int DepartmentId { get; set; }
        public List<Department>? Departments { get; set; }
       
        [Required(ErrorMessage = "DoctorRequired")]
        public int DoctorId { get; set; }
        public List<User>? Doctors { get; set; }
        public int? CashPayment { get; set; }
        public int? CashlessPayment { get; set; }
        public int? TypeOfCashlessPaymentId { get; set; }
        public List<TypeOfCashlessPayment>? TypesOfCashlessPayment { get; set; }
        public int BeenPaid { get; set; }
        public int Debt { get; set; }
        public int SumWithDiscount { get; set; }
        public int TotalCost { get; set; }
        public int? DiscountId { get; set; }
        public List<Discount>? Discounts { get; set; }
        public List<TypeOfDiagnostics>? TypesOfDiagnostics { get; set; } //все виды для страницы
        [Required(ErrorMessage = "TypesOfDiagnosticsRequired")]
        public List<int> TypesOfDiagnosticsId { get; set; }
        [Required(ErrorMessage = "AnamnesisRequired")]
        public string Anamnesis { get; set; }
        [Required(ErrorMessage = "ResearchMethodRequired")]
        public int ResearchMethodId { get; set; }
        public List<ResearchMethod>? ResearchMethods { get; set; }
    }
}
