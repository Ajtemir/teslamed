using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeslaMed.Models;
using TeslaMed.Models.Repositories;
using TeslaMed.ViewModels;


namespace TeslaMed.Controllers
{
    [Authorize]
    public class ManagerialController : Controller
    {
        public readonly TeslaMedContext _context;
        private readonly UserManager<User> _userManager;
        public readonly IRepository _repo;
        private readonly IStringLocalizer<ReportController> _localizer;

        public ManagerialController(TeslaMedContext context, IStringLocalizer<ReportController> localizer, IRepository repo, UserManager<User> userManager)
        {
            _context = context;
            _localizer = localizer;
            _repo = repo;
            _userManager = userManager;
        }
        public IActionResult ShowManagerialMainPage()
        {
            return View();
        }
        public IActionResult ShowPayDeskMainPage()
        {
            return View();
        }
        public IActionResult ShowStatisticsMainPage()
        {
            return View();
        }

                public async Task<IActionResult> AccountingByResearchMethod(DateTime? firstDate, DateTime? secondDate, int? researchMethod)
        {
            var diagnostics = _context.Diagnostics
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                .Include(d => d.Doctor).ThenInclude(d => d.Departments)
                .Include(d => d.Discount)
                .Include(d => d.ArrivalType)
                .Include(d => d.ArrivalTypeDoctor)
                .Include(d => d.Patient).AsQueryable();
            var researchMethods = _context.ResearchMethods.ToList();
            DateTime todayFrom = DateTime.Now.Date.AddHours(8);
            DateTime todayTo = DateTime.Now.Date.AddDays(1).AddHours(8);
            if (firstDate == null && secondDate == null && researchMethod == null)
                diagnostics = diagnostics.Where(d => d.TimeArrival >= todayFrom && d.TimeArrival <= todayTo);
            if (firstDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival >= firstDate.Value.AddHours(8));
            if (secondDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival <= secondDate.Value.AddDays(1).AddHours(8));
            if (researchMethod != null)
                researchMethods.RemoveAll(rm => rm.Id != researchMethod);
            List<AccountingByResearchMethodViewModel> accountingByRM = new List<AccountingByResearchMethodViewModel>();
            foreach (var rm in researchMethods)
            {
                AccountingByResearchMethodViewModel newMethod = new AccountingByResearchMethodViewModel()
                {
                    CashIncomeSum = 0,
                    CashlessIncomeSum = 0,
                    TotalIncomeSum = 0,
                };
                newMethod.ResearchMethodId = rm.Id;
                newMethod.ResearchMethod = rm;
                List<Diagnostics> diagnostics1 = diagnostics.Where(d => d.TypesOfDiagnostics.Any(t => t.ResearchMethod.Id == rm.Id)).ToList();
                for(int i = 0; i < diagnostics1.Count(); i++)
                {
                    newMethod.CashIncomeSum += diagnostics1[i].CashPayment;
                    newMethod.CashlessIncomeSum += diagnostics1[i].CashlessPayment;
                }
                newMethod.TotalIncomeSum = newMethod.CashIncomeSum + newMethod.CashlessIncomeSum;
                accountingByRM.Add(newMethod);
            }
            ViewBag.ResearchMethods = _context.ResearchMethods.ToList();
            return View(accountingByRM);
        }

        public async Task<IActionResult> LaboratoryAssistantStatistics(DateTime? firstDate, DateTime? secondDate, int? assistentName)
        {
            var users = await _context.Users.ToListAsync();
            List<User> assistants = new List<User>();
            List<User> assistantsForViewbag = new List<User>();
            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "x-ray_laboratory_assistant"))
                {
                    assistants.Add(user);
                    assistantsForViewbag.Add(user);
                }
            }
            if (assistentName != null)
                assistants.RemoveAll(a => a.Id != assistentName);
            var researchMethods = await _context.ResearchMethods.ToListAsync();
            ViewBag.ResearchMethods = researchMethods;
            ViewBag.Assistents = assistantsForViewbag;
            List<TotalRatingViewModel> totalRatings = new List<TotalRatingViewModel>();
            for(int i = 0; i < assistants.Count(); i++)
            {
                List<ResearchMethodRatingViewModel> researchMethodRatings = new List<ResearchMethodRatingViewModel>();
                double totalRatingCounter = 0;
                foreach (var researchMethod in researchMethods)
                {
                    ResearchMethodRatingViewModel rMRatingViewModel = new ResearchMethodRatingViewModel();
                    rMRatingViewModel.ResearchMethod = researchMethod;
                    rMRatingViewModel.ResearchMethodId = researchMethod.Id;
                    var diagnostics = _context.Diagnostics
                        .Include(d => d.TypesOfDiagnostics)
                        .Include(d => d.DicomPathAndImagesPaths)
                        .Where(d => d.LaborantId == assistants[i].Id && d.TypesOfDiagnostics.Any(t => t.ResearchMethod.Id == researchMethod.Id)).AsQueryable();
                    DateTime todayFrom = DateTime.Now.Date.AddHours(8);
                    DateTime todayTo = DateTime.Now.Date.AddDays(1).AddHours(8);
                    if (firstDate == null && secondDate == null && assistentName == null)
                        diagnostics = diagnostics.Where(d => d.TimeArrival >= todayFrom && d.TimeArrival <= todayTo);
                    if (firstDate != null)
                        diagnostics = diagnostics.Where(d => d.TimeArrival >= firstDate.Value.AddHours(8));
                    if (secondDate != null)
                        diagnostics = diagnostics.Where(d => d.TimeArrival <= secondDate.Value.AddDays(1).AddHours(8));
                    List<Diagnostics> diagnosticsToList = diagnostics.ToList();
                    int counterForDiagnostics = 0;
                    if(diagnosticsToList.Count != 0)
                    {
                        for (int j = 0; j < diagnosticsToList.Count(); j++)
                        {
                            for (int a = 0; a < diagnosticsToList[j].DicomPathAndImagesPaths.Count(); a++)
                            {
                                if (diagnosticsToList[j].DicomPathAndImagesPaths[a].Rating != null)
                                {
                                    rMRatingViewModel.AverageRating += (double)diagnosticsToList[j].DicomPathAndImagesPaths[a].Rating;
                                    counterForDiagnostics++;
                                }
                            }
                        }
                        rMRatingViewModel.AverageRating = rMRatingViewModel.AverageRating / counterForDiagnostics;
                    }
                    researchMethodRatings.Add(rMRatingViewModel);
                    totalRatingCounter += rMRatingViewModel.AverageRating;
                }
                TotalRatingViewModel totalRating = new TotalRatingViewModel()
                {
                    ResearchMethodRatings = researchMethodRatings,
                    AssistentName = assistants[i].Surname + " " + assistants[i].Name + " " + assistants[i].Patronymic,
                    TotalRating = totalRatingCounter / researchMethods.Count
                };
                totalRatings.Add(totalRating);
            }
            totalRatings = totalRatings.OrderByDescending(r => r.TotalRating).ToList();
            return View(totalRatings);
        }
        

        public async Task<IActionResult> AccountingByCosts(DateTime? firstDate, DateTime? secondDate)
        {
            var costs = _context.OperatingCosts.Include(c => c.OperatingCostName).Include(c => c.TypeOfCosts).AsQueryable(); ;
            DateTime todayFrom = DateTime.Now.Date.AddHours(8);
            DateTime todayTo = DateTime.Now.Date.AddDays(1).AddHours(8);
            if (firstDate == null && secondDate == null)
                costs = costs.Where(d => d.DateOfCreation >= todayFrom && d.DateOfCreation <= todayTo);
            if (firstDate != null)
                costs = costs.Where(d => d.DateOfCreation >= firstDate.Value.AddHours(8));
            if (secondDate != null)
                costs = costs.Where(d => d.DateOfCreation <= secondDate.Value.AddDays(1).AddHours(8));
            return View(await costs.ToListAsync());
        }

        public async Task<IActionResult> PreRegistrationStatistics (DateTime? firstDate, DateTime? secondDate, int? researchMethod)
        {
            var preEntries = _context.Entries.Include(p => p.TypeOfDiagnostics).AsQueryable();
            var researchMethods = _context.ResearchMethods.ToList();

            if (firstDate == null && secondDate == null)
                preEntries = preEntries.Where(p => p.StartTime.AddHours(-8).Year == DateTime.Now.Year);
            if (firstDate != null)
                preEntries = preEntries.Where(p => p.StartTime >= firstDate.Value.AddHours(8));
            if (secondDate != null)
                preEntries = preEntries.Where(p => p.StartTime <= secondDate.Value.AddDays(1).AddHours(8));
            if (researchMethod != null)
            {
                preEntries = preEntries.Where(p => p.TypeOfDiagnostics.ResearchMethodId == researchMethod);
                ViewBag.Filter = researchMethods.Where(m => m.Id == researchMethod);
            }

            ViewBag.ResearchMethods = researchMethods;
            return View(await preEntries.ToListAsync());
        }
        
        public async Task<IActionResult> AccountingForThisMonth()
        {
            int thisMonth = DateTime.Now.Month;
            int thisYear = DateTime.Now.Year;
            int daysInThisMonth = DateTime.DaysInMonth(thisYear, thisMonth);
            var diagnostics = _context.Diagnostics
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                .Include(d => d.Doctor).ThenInclude(d => d.Departments)
                .Include(d => d.Discount)
                .Include(d => d.ArrivalType)
                .Include(d => d.ArrivalTypeDoctor)
                .Include(d => d.Patient).Where(d => d.TimeArrival.Month == thisMonth).AsQueryable();
            var operationExpenses = _context.OperatingCosts.Where(o => o.DateOfCreation.Month == thisMonth).AsQueryable();
            List<AccountingForThisMonthViewModel> allDaysSums = new List<AccountingForThisMonthViewModel>();
            for(int i = 1; i <= daysInThisMonth; i++)
            {
                var thisDayDiagnostics = diagnostics.Where(d => d.TimeArrival.Date.Day == i);
                var thisDayExpenses = operationExpenses.Where(o => o.DateOfCreation.Day == i);
                AccountingForThisMonthViewModel thisDay = new AccountingForThisMonthViewModel();
                string dateString = i.ToString() + "/" + thisMonth.ToString() + "/" + thisYear.ToString();
                thisDay.date = Convert.ToDateTime(dateString);
                if (thisDayDiagnostics != null)
                {
                    foreach(var diagnostic in thisDayDiagnostics)
                    {
                        thisDay.CashIncomeSum += diagnostic.CashPayment;
                        thisDay.CashlessIncomeSum += diagnostic.CashlessPayment;
                    }
                }
                if(thisDayExpenses != null)
                {
                    foreach (var expense in thisDayExpenses)
                    {
                        thisDay.CashExpensesSum += expense.TotalAmount;
                    }
                }
                thisDay.TotalIncomeSum = thisDay.CashIncomeSum + thisDay.CashlessIncomeSum;
                thisDay.TotalProfitSum = thisDay.TotalIncomeSum - thisDay.CashExpensesSum;
                allDaysSums.Add(thisDay);
            }
            return View(allDaysSums);
        }
        public async Task<IActionResult> DiagnosticStatistics (DateTime? firstDate, DateTime? secondDate, int? researchMethod)
        {
            var researchMethods = await _context.ResearchMethods.ToListAsync();
            var diagnostics = _context.Diagnostics
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod).AsQueryable();

            if (firstDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival >= firstDate.Value.AddHours(8));
            if (secondDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival <= secondDate.Value.AddDays(1).AddHours(8));

            var rankedDiagList = new List<Tuple<string, string>>();
            foreach (var method in researchMethods)
            {
                int typesTotalCount = 0;
                foreach (var diag in diagnostics.Where(d => d.TypesOfDiagnostics.FirstOrDefault().ResearchMethod == method))
                    typesTotalCount += diag.TypesOfDiagnostics.Count;
                rankedDiagList.Add(new Tuple<string, string>($"{method.Name}", $"{typesTotalCount}"));
            }
            rankedDiagList = rankedDiagList.OrderByDescending(d => d.Item2).ToList();

            if (researchMethod != null)
            {
                ViewBag.Filter = rankedDiagList.IndexOf(rankedDiagList.FirstOrDefault(r => r.Item1 == researchMethods.FirstOrDefault(m => m.Id == researchMethod).Name));
                rankedDiagList = rankedDiagList.Where(d => d == rankedDiagList[ViewBag.Filter]).ToList();
            }

            ViewBag.ResearchMethods = researchMethods;
            return View(rankedDiagList);
        }

        public async Task<IActionResult> MonthlyCashRegister(int? month)
        {
            var diagnostics = _context.Diagnostics.Where(d => d.TimeArrival.AddHours(-8).Year == DateTime.Now.Year).AsQueryable();
            var operatingCosts = _context.OperatingCosts.Where(o => o.DateOfCreation.AddHours(-8).Year == DateTime.Now.Year).AsQueryable();

            if (month != null)
            {
                diagnostics = diagnostics.Where(d => d.TimeArrival.AddHours(-8).Month == month);
                operatingCosts = operatingCosts.Where(o => o.DateOfCreation.AddHours(-8).Month == month);
                ViewBag.Filter = month;
            }

            var viewModel = new OperatingCostsAndDiagnosticsViewModel
            {
                Diagnostics = diagnostics,
                OperatingCosts = operatingCosts
            };
            return View(viewModel);
        }

        public async Task<IActionResult> ArrivalTypeStatistics(DateTime? firstDate, DateTime? secondDate, int? arrivalType)
        {
            var diagnostics = _context.Diagnostics
                .Include(d => d.ArrivalType)
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod).AsQueryable();

            if (firstDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival >= firstDate.Value.AddHours(8));
            if (secondDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival <= secondDate.Value.AddDays(1).AddHours(8));
            if (arrivalType != null)
            {
                diagnostics = diagnostics.Where(d => d.ArrivalTypeId == arrivalType);
                ViewBag.Filter = diagnostics.Select(d => d.ArrivalType).Distinct();
            }

            ViewBag.ArrivalTypes = await _context.ArrivalTypes.ToListAsync();
            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            return View(await diagnostics.ToListAsync());
        }
        
        public async Task<IActionResult> FinancialTransactionStatistics (DateTime? firstDate, DateTime? secondDate, int? type)
        {
            var diagnostics = _context.Diagnostics
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                .Include(d => d.Discount).AsQueryable();

            if (firstDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival >= firstDate.Value.AddHours(8));
            if (secondDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival <= secondDate.Value.AddDays(1).AddHours(8));
            ViewBag.Filter = (type == 1 || type == 2) ? type : null;

            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            ViewBag.CashlessPatments = await _context.TypesOfCachlessPayment.ToListAsync();
            ViewBag.Discounts = await _context.Discounts.ToListAsync();
            return View(await diagnostics.ToListAsync());
        }

        public async Task<IActionResult> ComparativeIncomeStatistics(DateTime? firstDate, DateTime? secondDate, DateTime? thirdDate, DateTime? fourthDate)
        {
            var diagnostics = _context.Diagnostics.AsQueryable();
            var operatingCosts = _context.OperatingCosts.AsQueryable();
            var viewModel1 = new OperatingCostsAndDiagnosticsViewModel
            {
                Diagnostics = diagnostics,
                OperatingCosts = operatingCosts
            };
            var viewModel2 = new OperatingCostsAndDiagnosticsViewModel
            {
                Diagnostics = diagnostics,
                OperatingCosts = operatingCosts
            };
            
            if (firstDate != null)
            {
                viewModel1.Diagnostics = viewModel1.Diagnostics.Where(d => d.TimeArrival >= firstDate.Value.AddHours(8));
                viewModel1.OperatingCosts = viewModel1.OperatingCosts.Where(o => o.DateOfCreation >= firstDate.Value.AddHours(8));
            }
            if (secondDate != null)
            {
                viewModel1.Diagnostics = viewModel1.Diagnostics.Where(d => d.TimeArrival <= secondDate.Value.AddDays(1).AddHours(8));
                viewModel1.OperatingCosts = viewModel1.OperatingCosts.Where(o => o.DateOfCreation <= secondDate.Value.AddDays(1).AddHours(8));
            }
            if (thirdDate != null)
            {
                viewModel2.Diagnostics = viewModel2.Diagnostics.Where(d => d.TimeArrival >= thirdDate.Value.AddHours(8));
                viewModel2.OperatingCosts = viewModel2.OperatingCosts.Where(o => o.DateOfCreation >= thirdDate.Value.AddHours(8));
            }
            if (fourthDate != null)
            {
                viewModel2.Diagnostics = viewModel2.Diagnostics.Where(d => d.TimeArrival <= fourthDate.Value.AddDays(1).AddHours(8));
                viewModel2.OperatingCosts = viewModel2.OperatingCosts.Where(o => o.DateOfCreation <= fourthDate.Value.AddDays(1).AddHours(8));
            }
            var statistics = new List<OperatingCostsAndDiagnosticsViewModel> { viewModel1, viewModel2 };

            return View(statistics);
        }

        public async Task<IActionResult> ComparativeStatisticsDiagnostics (DateTime? firstDate, DateTime? secondDate, DateTime? thirdDate, DateTime? fourthDate)
        {
            var diagnostics = _context.Diagnostics
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod).AsQueryable();
            var diagnosics2 = diagnostics;

            if (firstDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival >= firstDate.Value.AddHours(8));
            if (secondDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival <= secondDate.Value.AddDays(1).AddHours(8));
            if (thirdDate != null)
                diagnosics2 = diagnosics2.Where(d => d.TimeArrival >= thirdDate.Value.AddHours(8));
            if (fourthDate != null)
                diagnosics2 = diagnosics2.Where(d => d.TimeArrival <= fourthDate.Value.AddDays(1).AddHours(8));

            var allDiagnosics = new List<IQueryable<Diagnostics>>{ diagnostics, diagnosics2 };
            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            return View(allDiagnosics);
        }
        
        public async Task<IActionResult> CashRegisterForACertainPeriodOfTime(DateTime? firstDate, DateTime? secondDate)
        {
            var diagnostics = _context.Diagnostics
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                .Include(d => d.Doctor).ThenInclude(d => d.Departments)
                .Include(d => d.Discount)
                .Include(d => d.ArrivalType)
                .Include(d => d.ArrivalTypeDoctor)
                .Include(d => d.Patient).AsQueryable();

            var operatingCosts = _context.OperatingCosts
                .Include(c => c.OperatingCostName)
                .Include(c => c.TypeOfCosts).AsQueryable();
            var listDiagnostics = diagnostics.OrderBy(d => d.TimeArrival).ToList();
            int totalCash = 0;
            int totalCashless = 0;
            int cashExpenditure = 0;
            int totalCost = 0;
            int cashProfit = 0;
            int cashlessProfit = 0;
            if (firstDate != null)
            {
                diagnostics = diagnostics.Where(d => d.TimeArrival >= firstDate);
                operatingCosts = operatingCosts.Where(o => o.DateOfCreation >= firstDate);
                ViewBag.DateFilterFrom = firstDate;
            }
            if (secondDate != null)
            {
                diagnostics = diagnostics.Where(d => d.TimeArrival <= secondDate.Value.AddDays(1));
                operatingCosts = operatingCosts.Where(o => o.DateOfCreation <= secondDate.Value.AddDays(1));
                ViewBag.DateFilterTo = secondDate;
            }
            foreach (var item in diagnostics)
            {
                if (item.CashPayment.HasValue)
                {
                    totalCash += (int)item.CashPayment;
                    totalCost += (int)item.CashPayment;
                }
                if (item.CashlessPayment.HasValue)
                {
                    totalCashless += (int)item.CashlessPayment;
                    totalCost += (int)item.CashlessPayment;
                }
            }
            foreach (var item in operatingCosts) 
            {
                cashExpenditure += item.TotalAmount;
            }
            cashProfit = totalCash - cashExpenditure;
            cashlessProfit = totalCashless;
            int nullDiagnostics = 0;
            if (listDiagnostics.Count == 0)
                nullDiagnostics = 1;
            Dictionary<string, int> cashRegister = new Dictionary<string, int>
            {
                {"totalCash", totalCash},
                {"totalCashless", totalCashless},
                {"cashExpenditure", cashExpenditure},
                {"totalCost", totalCost},
                {"cashProfit", cashProfit},
                {"cashlessProfit", cashlessProfit},
                {"totalProfit", cashProfit + cashlessProfit},
                {"diagnosticsNull", nullDiagnostics }
            };
            if (!firstDate.HasValue && listDiagnostics.Count != 0)
                ViewBag.DateFilterFrom = listDiagnostics[0].TimeArrival;
            if (!secondDate.HasValue && listDiagnostics.Count != 0)
                ViewBag.DateFilterTo = listDiagnostics[listDiagnostics.Count - 1].TimeArrival;
            return View(cashRegister);
        }
    }
}