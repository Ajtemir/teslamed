using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class PaymentViewModel
    {
        public int DiagnosticId { get; set; }

        [Display(Name = "CashAmount")]
        [Range(0, int.MaxValue, ErrorMessage = "AmountError")]
        public int? CashAmount { get; set; }

        [Display(Name = "CashlessAmount")]
        [Range(0, int.MaxValue, ErrorMessage = "AmountError")]
        public int? CashlessAmount { get; set; }
        public int? Debt { get; set; }
        public int? TypeOfCashlessPaymentId { get; set; }
        public TypeOfCashlessPayment? TypeOfCashlessPayment { get; set; }
        public List<TypeOfCashlessPayment>? TypesOfCashlessPayment { get; set; }
        public string Url { get; set; }
    }
}
