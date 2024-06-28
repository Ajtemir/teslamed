namespace TeslaMed.Models
{
    public class Diagnostics
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        public int? LaborantId { get; set; }
        public User? Laborant { get; set; }
        public string Anamnesis { get; set; }
        public List<TypeOfDiagnostics> TypesOfDiagnostics { get; set; }
        public DateTime TimeArrival { get; set; }
        public string Status { get; set; }
        public int Debt { get; set; }
        public int? BeenPaid { get; set; }
        public int TotalCost { get; set; }
        public int? CashPayment { get; set; }
        public int? CashlessPayment { get; set; }
        public int? TypeOfCashlessPaymentId { get; set; }
        public TypeOfCashlessPayment? TypeOfCashlessPayment { get; set; }
        public bool IsPaidInfull { get; set; }
        public string? LaboratoryAssistantComment { get; set; }
        public string? DoctorComment { get; set; }        
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public int? DoctorId { get; set; }
        public User? Doctor { get; set; }
        public int? DiscountId { get; set; }
        public Discount? Discount { get; set; }
        public int ArrivalTypeId { get; set; }
        public ArrivalType? ArrivalType { get; set; }
        public int? ArrivalTypeDoctorId { get; set; }
        public Doctor? ArrivalTypeDoctor { get; set; }
        public List<DicomPathAndImagesPath>? DicomPathAndImagesPaths { get; set; }
        public string ImagesStatus { get; set; }
        public int DiagnosticLogId { get; set; }
        public DiagnosticLog? DiagnosticLog { get; set; }
        public List<Comment> Comments { get; set; }
        public string? SmsDeliveryStatus { get; set; }
    }
}
