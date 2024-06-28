using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NuGet.Packaging;
using NuGet.Protocol;
using SixLabors.ImageSharp.ColorSpaces;
using TeslaMed.Models;
using TeslaMed.Models.Repositories;
using TeslaMed.ViewModels;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        public readonly TeslaMedContext _context;
        public readonly IRepository _repo;
        private readonly UserManager<User> _userManager;
        private readonly IStringLocalizer<ReportController> _localizer;

        public ReportController(TeslaMedContext context, IStringLocalizer<ReportController> localizer, IRepository repo, UserManager<User> userManager)
        {
            _context = context;
            _localizer = localizer;
            _repo = repo;
            _userManager = userManager;
        }

        public IActionResult ShowReportMainPage()
        {
            return View();
        }

        public async Task<IActionResult> GeneralReport(DateTime? firstDate, DateTime? secondDate, int? research, string? reportType)
        {
            int firstTimeLimit = 8;
            int secondTimeLimit = firstTimeLimit;
            int nextDayMorning = 1;
            ViewBag.ReportType = "General";

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

            if (reportType == "day")
            {
                nextDayMorning = 0;
                secondTimeLimit = 18;
                diagnostics = diagnostics.Where(d => d.TimeArrival.Hour >= firstTimeLimit && d.TimeArrival.Hour < secondTimeLimit);
                operatingCosts = operatingCosts.Where(c => c.DateOfCreation.Hour >= firstTimeLimit && c.DateOfCreation.Hour < secondTimeLimit);
            }
            else if (reportType == "night")
            {
                firstTimeLimit = 18;
                diagnostics = diagnostics.Where(d => d.TimeArrival.Hour >= firstTimeLimit || d.TimeArrival.Hour < secondTimeLimit);
                operatingCosts = operatingCosts.Where(c => c.DateOfCreation.Hour >= firstTimeLimit || c.DateOfCreation.Hour < secondTimeLimit);
            }

            if (firstDate != null)
            {
                diagnostics = diagnostics.Where(d => d.TimeArrival >= firstDate.Value.AddHours(firstTimeLimit));
                operatingCosts = operatingCosts.Where(c => c.DateOfCreation >= firstDate.Value.AddHours(firstTimeLimit));
                ViewBag.DateFilterFrom = firstDate.Value.AddHours(firstTimeLimit);
            }
            if (secondDate != null)
            {
                diagnostics = diagnostics.Where(d => d.TimeArrival <= secondDate.Value.AddDays(nextDayMorning).AddHours(secondTimeLimit));
                operatingCosts = operatingCosts.Where(c => c.DateOfCreation <= secondDate.Value.AddDays(nextDayMorning).AddHours(secondTimeLimit));
                ViewBag.DateFilterTo = secondDate.Value.AddDays(nextDayMorning).AddHours(secondTimeLimit);
            }
            if (research != null)
                diagnostics = diagnostics.Where(d => d.TypesOfDiagnostics.Any(t => t.ResearchMethod.Id == research));
            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            var diagnosticsAndOperatingCosts = new DiagnosticsAndOperatingCosts()
            {
                Diagnostics = diagnostics.ToList(),
                OperatingCosts = operatingCosts.ToList()
            };
            return View(diagnosticsAndOperatingCosts);
        }

        public async Task<IActionResult> DailyReport(DateTime? firstDate, DateTime? secondDate, int? research)
        {
            var result = (ViewResult)GeneralReport(firstDate, secondDate, research, "day").Result;
            ViewBag.ReportType = "Daily";
            return View("GeneralReport", result.Model);
        }

        public async Task<IActionResult> NightlyReport(DateTime? firstDate, DateTime? secondDate, int? research)
        {
            var result = (ViewResult)GeneralReport(firstDate, secondDate, research, "night").Result;
            ViewBag.ReportType = "Nightly";
            return View("GeneralReport", result.Model);
        }

        public async Task<IActionResult> StaffDoctorsSummaryReport (int? department, DateTime? firstDate, DateTime? secondDate)
        {
            var diagnostics = _context.Diagnostics
                .Include(d => d.Discount)
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                .Include(d => d.Doctor).ThenInclude(d => d.Departments)
                .Include(d => d.Doctor).ThenInclude(d => d.Post).AsQueryable();

            if (department != null)
            {
                diagnostics = diagnostics.Where(d => d.DepartmentId == department);
                ViewBag.DepartmentFilter = department;
            }
            if (firstDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival >= firstDate.Value.AddHours(8));
            if (secondDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival <= secondDate.Value.AddDays(1).AddHours(8));

            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.TypesOfDiagnostic = await _context.TypesOfDiagnostics.Include(t => t.ResearchMethod).ToListAsync();
            return View(await diagnostics.ToListAsync());
        }
        
        public async Task<IActionResult> DoctorReferralCountReport (int? place, int? doctor ,DateTime? firstDate, DateTime? secondDate)
        {
            var diagnostics = _context.Diagnostics
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                .Include(d => d.ArrivalTypeDoctor).ThenInclude(d => d.PlaceOfWork)
                .Include(d => d.Patient)
                .Where(d => d.ArrivalTypeDoctorId != null).AsQueryable();

            if (place != null)
                diagnostics = diagnostics.Where(d => d.ArrivalTypeDoctor.PlaceOfWorkId == place);
            if (doctor != null)
                diagnostics = diagnostics.Where(d => d.ArrivalTypeDoctorId == doctor);
            if (firstDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival >= firstDate.Value.AddHours(8));
            if (secondDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival <= secondDate.Value.AddDays(1).AddHours(8));
                
            ViewBag.PlacesOfWork = await _context.PlaceOfWorks.ToListAsync();
            ViewBag.Doctors = await _context.Doctors.Include(d => d.PlaceOfWork).ToListAsync();
            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            return View(await diagnostics.ToListAsync());
        }

        public async Task<IActionResult> DoctorReferralTotalReport(int? place, DateTime? firstDate, DateTime? secondDate)
        {
            var diagnostics = _context.Diagnostics
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                .Include(d => d.ArrivalTypeDoctor).ThenInclude(d => d.PlaceOfWork)
                .Include(d => d.Patient)
                .Where(d => d.ArrivalTypeDoctorId != null).AsQueryable();

            if (place != null)
                diagnostics = diagnostics.Where(d => d.ArrivalTypeDoctor.PlaceOfWorkId == place);
            if (firstDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival >= firstDate.Value.AddHours(8));
            if (secondDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival <= secondDate.Value.AddDays(1).AddHours(8));

            ViewBag.PlacesOfWork = await _context.PlaceOfWorks.ToListAsync();
            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            return View(await diagnostics.ToListAsync());
        }
        
        public async Task<IActionResult> DetailedReport(DateTime? firstDate, DateTime? secondDate, int? department, int? userDoctor, int? typeOfDiagnostics)
        {
            var diagnostics = _repo.GetAll<Diagnostics>("Diagnostics")
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                .Include(d => d.Doctor).ThenInclude(d => d.Departments)
                .Include(d => d.Discount)
                .Include(d => d.ArrivalType)
                .Include(d => d.ArrivalTypeDoctor)
                .Include(d => d.Patient).AsQueryable();

            if (firstDate != null)
            {
                diagnostics = diagnostics.Where(d => d.TimeArrival >= firstDate.Value.AddHours(8));
                ViewBag.DateFilterFrom = firstDate.Value.AddHours(8);
            }
            if (secondDate != null)
            {
                diagnostics = diagnostics.Where(d => d.TimeArrival <= secondDate.Value.AddDays(1).AddHours(8));
                ViewBag.DateFilterTo = secondDate.Value.AddDays(1).AddHours(8);
            }
            if (department != null)
                diagnostics = diagnostics.Where(d => d.Doctor.Departments.Any(y => y.Id == department));
            if (userDoctor != null)
                diagnostics = diagnostics.Where(d => d.Doctor.Id == userDoctor);
            if (typeOfDiagnostics != null)
                diagnostics = diagnostics.Where(d => d.TypesOfDiagnostics.Any(y => y.Id == typeOfDiagnostics));

            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.TypesOfDiagnostics = await _repo.GetAll<TypeOfDiagnostics>("TypesOfDiagnostics").Include(t => t.ResearchMethod)
                                .OrderBy(t => t.ResearchMethod.Name)
                                .ThenBy(t => t.Name).ToListAsync();
            var usersInDoctorRole = await _repo.GetUserByRole("doctor");
            ViewBag.UserDoctors = await _repo.GetAll<User>("Users").Where(d => usersInDoctorRole.Any(x => x == d)).Include(u => u.Departments).ToListAsync();
            return View(await diagnostics.ToListAsync());
        }
        
        public async Task<IActionResult> DetailedDepartmentReport(DateTime? firstDate, DateTime? secondDate, int? department)
        {
            var diagnostics = _context.Diagnostics
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                .Include(d => d.Department).AsQueryable();
                
            if (department != null)
                diagnostics = diagnostics.Where(d => d.DepartmentId == department);
            if (firstDate != null)
            {
                diagnostics = diagnostics.Where(d => d.TimeArrival >= firstDate.Value.AddHours(8));
                ViewBag.DateFilterFrom = firstDate.Value.AddHours(8);
            }
            if (secondDate != null)
            {
                diagnostics = diagnostics.Where(d => d.TimeArrival <= secondDate.Value.AddDays(1).AddHours(8));
                ViewBag.DateFilterTo = secondDate.Value.AddDays(1).AddHours(8);
            }            
                
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(await diagnostics.ToListAsync());
        }

        public async Task<IActionResult> ManagerReport(int? manager, int? place, int? doctor, DateTime? firstDate, DateTime? secondDate)
        {
            var diagnostics = _context.Diagnostics
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                .Include(d => d.Department)
                .Include(d => d.ArrivalTypeDoctor).ThenInclude(a => a.Manager).AsQueryable();

            if (manager != null)
                diagnostics = diagnostics.Where(d => d.ArrivalTypeDoctor.ManagerId == manager);
            if (place != null)
                diagnostics = diagnostics.Where(d => d.ArrivalTypeDoctor.PlaceOfWorkId == place);
            if (doctor != null)
                diagnostics = diagnostics.Where(d => d.ArrivalTypeDoctorId == doctor);
            if (firstDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival >= firstDate.Value.AddHours(8));
            if (secondDate != null)
                diagnostics = diagnostics.Where(d => d.TimeArrival <= secondDate.Value.AddDays(1).AddHours(8));

            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            ViewBag.TypesOfDiagnostic = await _context.TypesOfDiagnostics.Include(t => t.ResearchMethod).ToListAsync();
            ViewBag.Doctors = await _context.Doctors.Include(d => d.Manager).Include(d => d.PlaceOfWork).ToListAsync();
            var usersInManagerRole = await _userManager.GetUsersInRoleAsync("manager");
            usersInManagerRole.AddRange(await _userManager.GetUsersInRoleAsync("admin"));
            ViewBag.Managers = usersInManagerRole;
            return View(await diagnostics.ToListAsync());
        }
    }
}
