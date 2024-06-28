namespace TeslaMed.ViewModels
{
    public class AccountingForThisMonthViewModel
    {
        public DateTime date { get; set; }
        public int? CashIncomeSum { get; set; }
        public int? CashlessIncomeSum { get; set; }
        public int? TotalIncomeSum { get; set; }
        public int? CashExpensesSum { get; set; }
        public int? TotalProfitSum { get; set; }
        public AccountingForThisMonthViewModel()
        {
            CashIncomeSum = 0;
            CashlessIncomeSum = 0;
            TotalIncomeSum = 0;
            CashExpensesSum = 0;
            TotalProfitSum = 0;
        }
    }
}
