using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TeslaMed.Models.Repositories;
using TeslaMed.Models;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using TeslaMed.ViewModels;
using Rotativa.AspNetCore.Options;
using Microsoft.CodeAnalysis;

namespace TeslaMed.Controllers
{
    public class PdfController : Controller
    {
        private readonly IRepository _repo;
        private readonly TeslaMedContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IStringLocalizer<PatientsController> _localizer;

        public PdfController(TeslaMedContext context, UserManager<User> userManager, IStringLocalizer<PatientsController> localizer, IRepository repo)
        {
            _context = context;
            _userManager = userManager;
            _localizer = localizer;
            _repo = repo;
        }
        public async Task<IActionResult> DownloadInfo(int diagnosticsId, int patientId, int rsId)
        {
            var patient = _context.Patients.Find(patientId);
            var diagnostics = _context.Diagnostics
                .Include(d => d.TypesOfDiagnostics)
                .Include(d => d.Doctor)
                .Include(d => d.ArrivalType)
                .Include(d => d.ArrivalTypeDoctor)
                .FirstOrDefault(d => d.Id == diagnosticsId);
            var researchMethod = _context.ResearchMethods.Find(rsId);
            byte[] imageBytes = System.IO.File.ReadAllBytes("wwwroot/images/blackLogo.png");
            string base64Image = Convert.ToBase64String(imageBytes);
            var viewModel = new PatientWithDiagnosticsViewModel
            {
                Diagnostics = diagnostics,
                Patient = patient,
                ResearchMethod = researchMethod,
                Base64Image = base64Image
            };
            return new ViewAsPdf("PatientInfo", viewModel)
            {
                FileName = "PatientInfo.pdf",
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait,
                CustomSwitches = "--disable-smart-shrinking"
            };
        }

        public async Task<IActionResult> DownloadCheck(int diagnosticsId, int patientId, int rsId)
        {
            var patient = _context.Patients.Find(patientId);
            var diagnostics = _context.Diagnostics
                .Include(d => d.TypesOfDiagnostics)
                .Include(d => d.Doctor)
                .Include(d => d.ArrivalType)
                .Include(d => d.ArrivalTypeDoctor)
                .Include(d => d.Discount)
                .FirstOrDefault(d => d.Id == diagnosticsId);
            var researchMethod = _context.ResearchMethods.Find(rsId);
            byte[] imageBytes = System.IO.File.ReadAllBytes("wwwroot/images/blackLogo.png");
            string base64Image = Convert.ToBase64String(imageBytes);
            int finalPayment = 0;
            foreach (var item in diagnostics.TypesOfDiagnostics)
            {
                finalPayment += item.Price;
            }
            int discount = finalPayment - diagnostics.TotalCost;
            var viewModel = new PatientWithDiagnosticsViewModel
            {
                Diagnostics = diagnostics,
                Patient = patient,
                ResearchMethod = researchMethod,
                Base64Image = base64Image,
                FinalSum = finalPayment,
                Discount = discount,
            };
            return new ViewAsPdf("Check", viewModel)
            {
                FileName = "Check.pdf",
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait,
                CustomSwitches = "--disable-smart-shrinking"
            };
        }

        public async Task<IActionResult> DownloadConclusion(int id, int diagnosticsId, int patientId)
        {
            var model = _context.DicomPathAndImagesPaths
                .Include(m => m.TypeOfDiagnostics).ThenInclude(m => m.ResearchMethod)
                .FirstOrDefault(m => m.Id == id);
            byte[] imageBytes = System.IO.File.ReadAllBytes("wwwroot/images/blackLogo.png");
            string base64Image = Convert.ToBase64String(imageBytes);
            var patient = _context.Patients.Find(patientId);
            var diagnostics = _context.Diagnostics
                .Include(d => d.TypesOfDiagnostics)
                .Include(d => d.Doctor)
                .Include(d => d.ArrivalType)
                .Include(d => d.ArrivalTypeDoctor)
                .Include(d => d.Discount)
                .FirstOrDefault(d => d.Id == diagnosticsId);
            DateTime birthDate = patient.BirthDate; 
            DateTime currentDate = DateTime.Now;
            TimeSpan age = currentDate - birthDate;
            int fullYears = (int)(age.TotalDays / 365.25); 

            var viewModel = new ConclusionAndImageViewModel
            {
                Diagnostics = diagnostics,
                Patient = patient,
                Main = model,
                Base64Image = base64Image,
                Age = fullYears,
            };
            return new ViewAsPdf("Conclusion", viewModel)
            {
                FileName = "Conclusion.pdf",
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait,
                CustomSwitches = "--disable-smart-shrinking"
            };
        }

    }
}
