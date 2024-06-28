using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeslaMed.Models;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class OperatingActivitiesController : Controller
    {
        public readonly TeslaMedContext _context;
        private readonly IStringLocalizer<OperatingActivitiesController> _localizer;
        public OperatingActivitiesController(TeslaMedContext context, IStringLocalizer<OperatingActivitiesController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }
        public async Task<IActionResult> ShowOperatingActivitiesMainPage()
        {
            return View();
        }
        public async Task<IActionResult> IndexOperatingCosts()
        {
            DateTime todayFrom = DateTime.Now.Date.AddHours(8);
            DateTime todayTo = DateTime.Now.Date.AddDays(1).AddHours(8);
            var costs = _context.OperatingCosts.Include(c => c.OperatingCostName).Include(c => c.TypeOfCosts)
                .Where(c => c.IsManagersCost == false && c.DateOfCreation >= todayFrom && c.DateOfCreation <= todayTo).ToList();
            return View(costs);
        }
        [HttpPost]
        public async Task<IActionResult> IndexOperatingCosts(DateTime dateFilter, string nameFilter)
        {
            IQueryable<OperatingCost> costs = _context.OperatingCosts.Include(c => c.OperatingCostName).Include(c => c.TypeOfCosts)
                .Where(c => c.IsManagersCost == false);
            if (dateFilter != default)
                costs = costs.Where(c => c.DateOfCreation.Date == dateFilter.Date);
            if (nameFilter != null)
                costs = costs.Where(c => c.OperatingCostName.Name.ToLower().Contains(nameFilter.ToLower()));
            ViewBag.Filtering = true;
            return View(costs.ToList());
        }
        [HttpGet]
        public IActionResult CreateOperatingCosts(bool isManagersCost)
        {
            var names = _context.OperatingCostNames.ToList();
            var types = _context.TypesOfCosts.ToList();
            ViewBag.OperatingCostNames = names;
            ViewBag.TypesOfCosts = types;
            ViewBag.IsManagersCost = isManagersCost;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateOperatingCosts(OperatingCost newCost)
        {
            var names = _context.OperatingCostNames.ToList();
            var types = _context.TypesOfCosts.ToList();
            ViewBag.OperatingCostNames = names;
            ViewBag.TypesOfCosts = types;
            if (ModelState.IsValid)
            {
                var cost = await _context.OperatingCosts.AsNoTracking().FirstOrDefaultAsync(i => i.OperatingCostNameId == newCost.OperatingCostNameId && i.DateOfCreation.Date == DateTime.Now.Date && i.IsManagersCost == newCost.IsManagersCost);
                if (cost != null)
                {
                    cost.TotalAmount += newCost.TotalAmount;
                    _context.OperatingCosts.Update(cost);
                    await _context.SaveChangesAsync();
                    if (cost.IsManagersCost)
                        return RedirectToAction("AccountingByCosts", "Managerial");
                    return RedirectToAction("IndexOperatingCosts");
                }
                newCost.DateOfCreation = DateTime.Now;
                await _context.OperatingCosts.AddAsync(newCost);
                await _context.SaveChangesAsync();
                if (newCost.IsManagersCost)
                    return RedirectToAction("AccountingByCosts", "Managerial");
                return RedirectToAction("IndexOperatingCosts");
            }
            return View(newCost);
        }
        [HttpGet]
        public async Task<IActionResult> OperationalProcess(DateTime? firstDate, DateTime? secondDate, string patientInfo, int? research)
        {
            var diagnostics = _context.Diagnostics
                .Include(d => d.Laborant)
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                .Include(d => d.Doctor)
                .Include(d => d.Discount)
                .Include(d => d.ArrivalType)
                .Include(d => d.Patient)
                .Include(d => d.DiagnosticLog).AsQueryable();          

            if (firstDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival >= firstDate.Value.AddHours(8));
            if (secondDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival < secondDate.Value.AddDays(1).AddHours(8));
            if (patientInfo != null)
                diagnostics = diagnostics.Where(d => (d.Patient.Surname + d.Patient.Name + d.Patient.Patronymic).ToLower().Contains(patientInfo.Replace(" ", "").ToLower()));
            if (research != null)
                diagnostics = diagnostics.Where(d => d.TypesOfDiagnostics.Any(t => t.ResearchMethod.Id == research));
            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            return View(await diagnostics.ToListAsync());

        }
    }
}
