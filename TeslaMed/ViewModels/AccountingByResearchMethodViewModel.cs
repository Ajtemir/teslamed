using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class AccountingByResearchMethodViewModel
    {
        public int ResearchMethodId { get; set; }
        public ResearchMethod ResearchMethod { get; set; }
        public int? CashIncomeSum { get; set; }
        public int? CashlessIncomeSum { get; set; }
        public int? TotalIncomeSum { get; set; }
        
    }
}
