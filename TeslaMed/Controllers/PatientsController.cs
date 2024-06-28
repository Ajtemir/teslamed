using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using MessagePack.Formatters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Localization;
using SixLabors.ImageSharp;
using TeslaMed.Models;
using TeslaMed.ViewModels;
using TeslaMed.Models.Repositories;
using System.Collections;
using Microsoft.CodeAnalysis;
using Rotativa.AspNetCore;
using TeslaMed.Services;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class PatientsController : Controller
    {
        private readonly IRepository _repo;
        private readonly TeslaMedContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IStringLocalizer<PatientsController> _localizer;

        public PatientsController(TeslaMedContext context, UserManager<User> userManager, IStringLocalizer<PatientsController> localizer, IRepository repo)
        {
            _context = context;
            _userManager = userManager;
            _localizer = localizer;
            _repo = repo;
        }


        [HttpGet]
        public async Task<IActionResult> Index(DateTime? filterDate, string filterFirstName, string filterLastName, string filterPatronymic, bool resetFilters = false)
        {
            ViewBag.filterFirstName = filterFirstName;
            ViewBag.filterLastName = filterLastName;
            ViewBag.filterPatronymic = filterPatronymic;

            var today = DateTime.Now.Date;
            var query = _context.Patients.AsQueryable();

            if (resetFilters)
            {
                ViewBag.filterDate = null;
            }
            else if (filterDate.HasValue)
            {
                query = query.Where(p => p.CreationDate.Date == filterDate.Value.Date);
                ViewBag.filterDate = filterDate.Value.ToString("yyyy-MM-dd");
            }
            else if (!string.IsNullOrEmpty(filterFirstName) || !string.IsNullOrEmpty(filterLastName) || !string.IsNullOrEmpty(filterPatronymic))
            {
                ViewBag.filterDate = null;
            }
            else
            {
                query = query.Where(p => p.CreationDate.Date == today);
                ViewBag.filterDate = today.ToString("yyyy-MM-dd");
            }

            if (!string.IsNullOrEmpty(filterFirstName))
            {
                filterFirstName = filterFirstName.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(filterFirstName));
            }

            if (!string.IsNullOrEmpty(filterLastName))
            {
                filterLastName = filterLastName.Trim().ToLower();
                query = query.Where(p => p.Surname.ToLower().Contains(filterLastName));
            }

            if (!string.IsNullOrEmpty(filterPatronymic))
            {
                filterPatronymic = filterPatronymic.Trim().ToLower();
                query = query.Where(p => p.Patronymic.ToLower().Contains(filterPatronymic));
            }

            var patients = await query.ToListAsync();
            return View(patients);
        }


        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientCreateViewModel patientCreateView)
        {
            if (ModelState.IsValid)
            {
                Patient patient = new Patient()
                {
                    Name = patientCreateView.Name,
                    Surname = patientCreateView.Surname,
                    Patronymic = patientCreateView.Patronymic,
                    BirthDate = patientCreateView.BirthDate.Date,
                    Gender = patientCreateView.Gender,
                    PhoneNumber = patientCreateView.PhoneNumber,
                    SecondPhoneNumber = patientCreateView.SecondPhoneNumber,
                    Comment = patientCreateView.Comment,
                    Code = GetRandomUniqueCode().Result
                };
                await _repo.DbAdd(patient);
                await _repo.DbSave();
                if (patientCreateView.CreateDiagnostics)
                {
                    var patientId = await _repo.GetAll<Patient>("Patients").FirstOrDefaultAsync(p => p.Code == patient.Code);

                    return RedirectToAction("DiagnosticsCreate", new { patientId?.Id });
                }
                else
                    return RedirectToAction(nameof(Index));
            }
            return View(patientCreateView);
        }

        [HttpPost]
        public async Task<IActionResult> PatientExistenceCheck(string name, string surname, string patronymic, string date)
        {
            var patients = await _repo.GetAll<Patient>("Patients").Where(p => p.Name == name && p.Surname == surname && p.Patronymic == patronymic && p.BirthDate == Convert.ToDateTime(date)).ToListAsync();
            if (patients.Count == 0)
                return Json(300);
            else
                return PartialView("PartialViews/ExistingPatientsListPartialView", patients);
        }

        public async Task<string> GetRandomUniqueCode()
        {
            string uniqueCode = GetRandomCode().Result;
            Patient? patient = await _repo.GetAll<Patient>("Patients").FirstOrDefaultAsync(p => p.Code == uniqueCode);
            if (patient != null)
                return await GetRandomUniqueCode();
            return uniqueCode;
        }
        public async Task<string> GetRandomCode()
        {
            string tmpCode = string.Empty;
            for (int i = 0; i < 5; i++)
            {
                int tmpNumber = new Random().Next(0, 9);
                tmpCode = tmpCode + tmpNumber.ToString();
            }
            return tmpCode;
        }
        [AcceptVerbs("GET", "POST")]
        public bool BirthDateCheck(DateTime birthDate)
        {
            if (birthDate > DateTime.Now.Date)
                return false;
            return true;
        }
        private bool PatientExists(int id)
        {
            return (_context.Patients?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();
            var patient = await _repo.GetAll<Patient>("Patients").FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
                return NotFound();
            var diagnostics = _repo.GetAll<Diagnostics>("Diagnostics")
                                      .Include(d => d.Laborant)
                                      .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                                      .Include(d => d.Doctor)
                                      .Include(d => d.Discount)
                                      .Include(d => d.ArrivalType)
                                      .Include(d => d.ArrivalTypeDoctor)
                                      .Include(d => d.DiagnosticLog)
                                      .Include(d => d.TypeOfCashlessPayment)
                                      .Where(d => d.PatientId == id).ToList();
            diagnostics = diagnostics.OrderByDescending(d => d.TimeArrival).ToList();
            var typesOfCashlessPayment = _repo.GetAll<TypeOfCashlessPayment>("TypesOfCachlessPayment").ToList();
            PatientDetailsViewModel patientDetailsVM = new PatientDetailsViewModel()
            {
                Patient = patient,
                Diagnostics = diagnostics,
                TypesOfCashlessPayment = typesOfCashlessPayment
            };
            return View(patientDetailsVM);
        }
        public async Task<IActionResult> AllDiagnostics(bool? allLaborantDiagnostics, string filterLastName, string filterFirstName, DateTime? filterDate, int? filterResearchMethod)
        {
            var query = _context.Diagnostics
                .Include(d => d.Laborant)
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                .Include(d => d.Doctor)
                .Include(d => d.Discount)
                .Include(d => d.ArrivalType)
                .Include(d => d.Patient)
                .AsQueryable();
            ViewBag.ResearchMethods = _context.ResearchMethods.ToList();
            ViewBag.TypesOfCashlessPayment = await _context.TypesOfCachlessPayment.ToListAsync();
            ViewBag.Entries = _repo.GetAll<Pre_entry>("Entries").ToList();
            Dictionary<DateTime, Diagnostics> dateDiagnosticsPairs = new Dictionary<DateTime, Diagnostics>();
            List<Diagnostics> diagnostics = new List<Diagnostics>();
            if (allLaborantDiagnostics == true)
            {
                query = query.Where(d => d.Laborant.UserName == User.Identity.Name || d.Laborant == null);
            }

            if (!string.IsNullOrEmpty(filterLastName))
            {
                query = query.Where(d => d.Patient.Surname.ToLower().Contains(filterLastName.ToLower()));
            }

            if (!string.IsNullOrEmpty(filterFirstName))
            {
                query = query.Where(d => d.Patient.Name.ToLower().Contains(filterFirstName.ToLower()));
            }

            if (filterResearchMethod.HasValue)
            {
                query = query.Where(d => d.TypesOfDiagnostics.Any(t => t.ResearchMethodId == filterResearchMethod.Value));
            }

            if (filterDate.HasValue)
            {
                query = query.Where(l => l.TimeArrival >= filterDate.Value.AddHours(8) && l.TimeArrival < filterDate.Value.AddDays(1).AddHours(8));
            }
            if(allLaborantDiagnostics == true)
            {
                diagnostics = query.ToList();
                diagnostics = diagnostics.Where(d => d.Laborant?.UserName == User.Identity.Name).ToList();
                diagnostics = diagnostics.OrderByDescending(d => d.TimeArrival).ToList();
                return View(diagnostics);
            }
            if (dateDiagnosticsPairs.Count == 0)
                diagnostics = query.OrderByDescending(d => d.TimeArrival).ToList();
            else
            {
                var sortedDict = new SortedDictionary<DateTime, Diagnostics>(dateDiagnosticsPairs);
                diagnostics = sortedDict.Values.ToList();
            }
            return View(diagnostics);
        }

        public async Task<IActionResult> DiagnosticsDetails(int id)
        {
            if (id == null)
                return BadRequest();
            var diagnostics = await _repo.GetAll<Diagnostics>("Diagnostics")
                .Include(d => d.Laborant)
                .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                .Include(d => d.Doctor)
                .Include(d => d.Discount)
                .Include(d => d.ArrivalType)
                .Include(d => d.ArrivalTypeDoctor)
                .Include(d => d.DiagnosticLog)
                .Include(d => d.TypeOfCashlessPayment)
                .Include(d => d.Patient)
                .Include(d => d.DicomPathAndImagesPaths).ThenInclude(d => d.TypeOfDiagnostics)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (diagnostics == null)
                return NotFound();

            if (diagnostics.SmsDeliveryStatus != null ? new[] { "SendStatus4", "SendStatus9", "Report0", "Report1", "ReportStatus5" }.Any(c => diagnostics.SmsDeliveryStatus.Contains(c)) : false)
            {
                SmsService smsService = new SmsService();
                if (diagnostics.SmsDeliveryStatus == "SendStatus4" || diagnostics.SmsDeliveryStatus == "SendStatus9")
                    diagnostics.SmsDeliveryStatus = smsService.SmsSend($"{diagnostics.Patient.Code}{diagnostics.Id}", diagnostics.Patient.PhoneNumber, diagnostics.Patient.SecondPhoneNumber).Result;
                if (diagnostics.SmsDeliveryStatus == "Report0" || diagnostics.SmsDeliveryStatus == "Report1" || diagnostics.SmsDeliveryStatus == "ReportStatus5")
                    diagnostics.SmsDeliveryStatus = smsService.SmsReport($"{diagnostics.Patient.Code}{diagnostics.Id}", (diagnostics.Patient.PhoneNumber != null && diagnostics.Patient.SecondPhoneNumber != null) ? true : false).Result;
                _context.Diagnostics.Update(diagnostics);
                await _context.SaveChangesAsync();
            }    

            var reseachMethodId = 0;
            foreach (var item in diagnostics.TypesOfDiagnostics)
            {
                reseachMethodId = item.ResearchMethodId;
            }
            ViewBag.ResearchMethodId = reseachMethodId;
            ViewBag.TypesOfCashlessPayment = await _repo.GetAll<TypeOfCashlessPayment>("TypesOfCachlessPayment").ToListAsync();
            ViewBag.DicomsAndImages =  await _context.DicomPathAndImagesPaths.ToListAsync();
            ViewBag.Entries = _repo.GetAll<Pre_entry>("Entries").Where(d => d.DiagnosticsId == id).ToList();
            return View(diagnostics);
        }
        [HttpPost]
        public async Task<IActionResult> RatePhoto(ReportViewModel reportViewModel)
        
        {
            if (reportViewModel.DiagnosticId == null || reportViewModel.TypeOfDiagnosticId == null)
                return BadRequest();
            var diagnostics = await _context.Diagnostics.Include(d => d.DicomPathAndImagesPaths).ThenInclude(d => d.TypeOfDiagnostics).FirstOrDefaultAsync(d => d.Id == reportViewModel.DiagnosticId);
            if (diagnostics == null)
                return NotFound();
            var dicomAndImagePath = diagnostics.DicomPathAndImagesPaths.FirstOrDefault(d => d.TypeOfDiagnosticsId == reportViewModel.TypeOfDiagnosticId);
            if (dicomAndImagePath == null)
                return NotFound();
            dicomAndImagePath.Rating = reportViewModel.Rating;
            _context.DicomPathAndImagesPaths.Update(dicomAndImagePath);
            await _context.SaveChangesAsync();
            return RedirectToAction("DiagnosticsDetails", new {id = reportViewModel.DiagnosticId});
        }
        [HttpPost]
        public async Task<IActionResult> Conclude(ReportViewModel reportViewModel)
        {
            SmsService smsService = new SmsService();
            if (reportViewModel.DiagnosticId == null || reportViewModel.TypeOfDiagnosticId == null)
                return BadRequest();
            var diagnostics = await _context.Diagnostics.Include(d => d.Patient).Include(d => d.DicomPathAndImagesPaths).ThenInclude(d => d.TypeOfDiagnostics).FirstOrDefaultAsync(d => d.Id == reportViewModel.DiagnosticId);
            if (diagnostics == null)
                return NotFound();
            var dicomAndImagePath = diagnostics.DicomPathAndImagesPaths.FirstOrDefault(d => d.TypeOfDiagnosticsId == reportViewModel.TypeOfDiagnosticId);
            if (dicomAndImagePath == null)
                return NotFound();

            if (diagnostics.Patient.PhoneNumber != null || diagnostics.Patient.SecondPhoneNumber != null)
                diagnostics.SmsDeliveryStatus = smsService.SmsSend($"{diagnostics.Patient.Code}{diagnostics.Id}", diagnostics.Patient.PhoneNumber, diagnostics.Patient.SecondPhoneNumber).Result;

            dicomAndImagePath.Conclusion = reportViewModel.Conclusion;
            _context.DicomPathAndImagesPaths.Update(dicomAndImagePath);
            await _context.SaveChangesAsync();
            return RedirectToAction("DiagnosticsDetails", new { id = reportViewModel.DiagnosticId });
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
                return NotFound();
            PatientCreateViewModel patientVM = new PatientCreateViewModel()
            {
                Name = patient.Name,
                Surname = patient.Surname,
                Patronymic = patient.Patronymic,
                BirthDate = patient.BirthDate,
                Gender = patient.Gender,
                PhoneNumber = patient.PhoneNumber,
                SecondPhoneNumber = patient.SecondPhoneNumber,
                Comment = patient.Comment,
            };
            ViewBag.Code = patient.Code;
            ViewBag.Id = patient.Id;
            return View(patientVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PatientCreateViewModel updPatient, string code, int id)
        {
            if (ModelState.IsValid)
            {
                Patient? patient = await _repo.GetAll<Patient>("Patients").AsNoTracking().FirstOrDefaultAsync(p => (p.Name == updPatient.Name && p.Surname == updPatient.Surname && p.Patronymic == updPatient.Patronymic && p.BirthDate == updPatient.BirthDate));
                if (patient != null && patient.Id != id)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(updPatient);
                }
                patient = new Patient()
                {
                    Name = updPatient.Name,
                    Surname = updPatient.Surname,
                    Patronymic = updPatient.Patronymic,
                    Gender = updPatient.Gender,
                    BirthDate = updPatient.BirthDate,
                    PhoneNumber = updPatient.PhoneNumber,
                    SecondPhoneNumber = updPatient.SecondPhoneNumber,
                    Comment = updPatient.Comment,
                    Code = code,
                    Id = id
                };
                _repo.DbUpdate(patient);
                await _repo.DbSave();
                return RedirectToAction("Details", new { id });
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(updPatient);
        }
        public async Task<IActionResult> DeletePatient(int id)
        {
            if (id == null)
                return BadRequest();
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
                return NotFound();
            if (patient.HasDiagnostics == true)
                return BadRequest();
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> DiagnosticsCreate(int? id)
        {
            if (id == null)
                return BadRequest();
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
                return NotFound();
            var doctors = await _context.Doctors.ToListAsync();
            DiagnosticsCreateViewModel diagnosticsCreateViewModel = new DiagnosticsCreateViewModel()
            {
                Departments = await _context.Departments.ToListAsync(),
                ArrivalTypeDoctors = doctors,
                ArrivalTypes = await _context.ArrivalTypes.ToListAsync(),
                TypesOfDiagnostics = await _context.TypesOfDiagnostics.Include(d => d.ResearchMethod).ToListAsync(),
                Discounts = await _context.Discounts.ToListAsync(),
                TypesOfCashlessPayment = await _context.TypesOfCachlessPayment.ToListAsync(),
                Patient = patient,
                PatientId = patient.Id,
                ResearchMethods = await _context.ResearchMethods.ToListAsync(),
                CashPayment = 0,
                CashlessPayment = 0
            };
            return View(diagnosticsCreateViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DiagnosticsCreate(DiagnosticsCreateViewModel diagnosticsCreateViewModel)
        {
            var users = await _context.Users.ToListAsync();
            List<User> doctors = new List<User>();
            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "doctor"))
                    doctors.Add(user);
            }
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == diagnosticsCreateViewModel.PatientId);
            diagnosticsCreateViewModel.Patient = patient;
            diagnosticsCreateViewModel.ArrivalTypes = await _context.ArrivalTypes.ToListAsync();
            diagnosticsCreateViewModel.Doctors = doctors;
            diagnosticsCreateViewModel.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            diagnosticsCreateViewModel.TypesOfCashlessPayment = await _context.TypesOfCachlessPayment.ToListAsync();
            diagnosticsCreateViewModel.ArrivalTypeDoctors = await _context.Doctors.ToListAsync();
            diagnosticsCreateViewModel.Departments = await _context.Departments.ToListAsync();
            diagnosticsCreateViewModel.Discounts = await _context.Discounts.ToListAsync();
            diagnosticsCreateViewModel.TypesOfDiagnostics = await _context.TypesOfDiagnostics.ToListAsync();
            if (ModelState.IsValid)
            {
                Discount? discount = await _context.Discounts.FirstOrDefaultAsync(d => d.Id == diagnosticsCreateViewModel.DiscountId);
                if (discount == null)
                    discount = new Discount() { Percent = 0 };
                List<int> TypeOfDiagnosticsId = diagnosticsCreateViewModel.TypesOfDiagnosticsId.ToList();
                if (TypeOfDiagnosticsId == null)
                    return BadRequest();
                List<TypeOfDiagnostics> typesOfDiagnostics = new List<TypeOfDiagnostics>();
                for (int i = 0; i < diagnosticsCreateViewModel.TypesOfDiagnosticsId.Count; i++)
                {
                    typesOfDiagnostics.Add(await _context.TypesOfDiagnostics.FirstOrDefaultAsync(d => d.Id == diagnosticsCreateViewModel.TypesOfDiagnosticsId[i]));                   
                }
                if (diagnosticsCreateViewModel.CashlessPayment == null)
                    diagnosticsCreateViewModel.CashlessPayment = 0;
                if (diagnosticsCreateViewModel.CashPayment == null)
                    diagnosticsCreateViewModel.CashPayment = 0;
                if (diagnosticsCreateViewModel.TypeOfCashlessPaymentId != null && diagnosticsCreateViewModel.CashlessPayment == 0)
                    diagnosticsCreateViewModel.TypeOfCashlessPaymentId = null;
                bool isPaidInfull = false;
                if (diagnosticsCreateViewModel.Debt == 0)
                    isPaidInfull = true;
                Diagnostics diagnostics = new Diagnostics()
                {
                    PatientId = diagnosticsCreateViewModel.PatientId,
                    DepartmentId = diagnosticsCreateViewModel.DepartmentId,
                    DoctorId = diagnosticsCreateViewModel.DoctorId,
                    ArrivalTypeId = diagnosticsCreateViewModel.ArrivalTypeId,
                    TypesOfDiagnostics = typesOfDiagnostics,
                    DiscountId = diagnosticsCreateViewModel?.DiscountId,
                    Status = "в процессе",
                    TotalCost = diagnosticsCreateViewModel.SumWithDiscount,
                    Debt = diagnosticsCreateViewModel.Debt,
                    BeenPaid = diagnosticsCreateViewModel.BeenPaid,
                    CashPayment = diagnosticsCreateViewModel.CashPayment,
                    CashlessPayment = diagnosticsCreateViewModel.CashlessPayment,
                    TypeOfCashlessPaymentId = diagnosticsCreateViewModel.TypeOfCashlessPaymentId,
                    Anamnesis = diagnosticsCreateViewModel.Anamnesis,
                    TimeArrival = DateTime.Now,
                    ArrivalTypeDoctorId = diagnosticsCreateViewModel.ArrivalTypeDoctorId,
                    IsPaidInfull = isPaidInfull,
                    ImagesStatus = "Available",
                    DiagnosticLog = new DiagnosticLog()
                };
                await _context.AddAsync(diagnostics);
                patient.HasDiagnostics = true;
                _context.Patients.Update(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["DiagnosticsRequired"]);
            return View(diagnosticsCreateViewModel);
        }

        [Authorize(Roles = "admin, registrar")]
        public async Task<IActionResult> DiagnosticsDelete (int? id)
        {
            if (id == null)
                return BadRequest();
            var diagnostic = await _context.Diagnostics.Include(d => d.Patient).FirstOrDefaultAsync(d => d.Id == id);
            if (diagnostic == null)
                return NotFound();
            _context.Diagnostics.Remove(diagnostic);
            await _context.SaveChangesAsync();
            var diagnostics = _context.Diagnostics.Any(d => d.PatientId == diagnostic.PatientId);
            
            if (diagnostics == false)
            {
                diagnostic.Patient.HasDiagnostics = false;
                _context.Patients.Update(diagnostic.Patient);
                await _context.SaveChangesAsync();
            }            
            return RedirectToAction("AllDiagnostics", new { filterDate = $"{DateTime.Now:MM.dd.yyyy}"});
        }

        public async Task<IActionResult> DoctorsList(int? id)
        {
            if (id == null)
                return BadRequest();
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
            if (department == null)
                return NotFound();
            var doctors = await _userManager.GetUsersInRoleAsync("doctor");
            DiagnosticsCreateViewModel diagnosticsCreateVM = new DiagnosticsCreateViewModel()
            {
                Doctors = _context.Users.Where(u => (doctors.Any(d => d == u) && u.Departments.Any(d => d.Id == id))).ToList()
            };
            return PartialView("PartialViews/DoctorSelectPartialView", diagnosticsCreateVM);
        }

        public async Task<IActionResult> TypesOfDiagnosticList(int? id)
        {
            if (id == null)
                return BadRequest();
            var researchMethod = await _repo.GetAll<ResearchMethod>("ResearchMethods").FirstOrDefaultAsync(d => d.Id == id);
            if (researchMethod == null)
                return NotFound();
            DiagnosticsCreateViewModel diagnosticsCreateVM = new DiagnosticsCreateViewModel()
            {
                TypesOfDiagnostics = await _repo.GetAll<TypeOfDiagnostics>("TypesOfDiagnostics").Include(t => t.ResearchMethod).Where(t => t.ResearchMethodId == id).ToListAsync()
            };
            return PartialView("PartialViews/TypeOfDiagnosticsSelectPartialView", diagnosticsCreateVM);
        }

        public async Task<IActionResult> Payment (PaymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var diagnostic = await _context.Diagnostics.FirstOrDefaultAsync(d => d.Id == model.DiagnosticId);
                if (diagnostic == null)
                    return NotFound();
                if (model.CashAmount == null)
                    model.CashAmount = 0;
                if (model.CashlessAmount == null)
                    model.CashlessAmount = 0;
                if (model.TypeOfCashlessPaymentId != null)
                    diagnostic.TypeOfCashlessPaymentId = model.TypeOfCashlessPaymentId;
                int amount = (int)(model.CashAmount + model.CashlessAmount);
                if (diagnostic.Debt < amount) 
                    return BadRequest();
                diagnostic.BeenPaid = (diagnostic.BeenPaid ?? 0) + amount;
                diagnostic.Debt -= amount;
                diagnostic.CashPayment = (diagnostic.CashPayment ?? 0) + model.CashAmount;
                diagnostic.CashlessPayment = (diagnostic.CashlessPayment ?? 0) + model.CashlessAmount;
                if (diagnostic.Debt <= 0)
                    diagnostic.IsPaidInfull = true;
                _context.Diagnostics.Update(diagnostic);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("DiagnosticsDetails", "Patients" , new {id = model.DiagnosticId});
        }
        [Authorize(Roles = "x-ray_laboratory_assistant, admin")]
        [HttpGet]
        public async Task<IActionResult> SaveDicomFile(int diagnosticsId, int typeOfDiagnosticsId)
        {
            if (diagnosticsId == null && typeOfDiagnosticsId == null)
                return BadRequest();    
            var diagnostics = await _context.Diagnostics
                                      .Include(d => d.Laborant)
                                      .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                                      .Include(d => d.Doctor)
                                      .Include(d => d.Discount)
                                      .Include(d => d.ArrivalType)
                                      .Include(d => d.Patient)
                                      .Include(d => d.ArrivalTypeDoctor)
                                      .Include(d => d.DicomPathAndImagesPaths).ThenInclude(d => d.TypeOfDiagnostics)
                                      .FirstOrDefaultAsync(d => d.Id == diagnosticsId);
            if (diagnostics == null)
                return NotFound();
            var model = new DicomFilesSaveViewModel()
            {
                Diagnostic = diagnostics,
                DiagnosticId = diagnosticsId,
                TypeOfDiagnosticsId = typeOfDiagnosticsId,
                TypeOfDiagnostics = await _context.TypesOfDiagnostics.FirstOrDefaultAsync(t => t.Id == typeOfDiagnosticsId)
            };
            return View(model);
        }
        [Authorize(Roles = "x-ray_laboratory_assistant, admin")]
        [HttpPost]
        public async Task<IActionResult> SaveDicomFile(DicomFilesSaveViewModel model)
        {
            
            if (model.DiagnosticId == null)
                return BadRequest();
            var diagnostics = await _context.Diagnostics
                                      .Include(d => d.Laborant)
                                      .Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod)
                                      .Include(d => d.Doctor)
                                      .Include(d => d.Discount)
                                      .Include(d => d.ArrivalType)
                                      .Include(d => d.Patient)
                                      .Include(d => d.ArrivalTypeDoctor)
                                      .Include(d => d.DicomPathAndImagesPaths).ThenInclude(d => d.TypeOfDiagnostics)
                                      .FirstOrDefaultAsync(d => d.Id == model.DiagnosticId);
            if (diagnostics == null)
                return NotFound();
            new DicomSetupBuilder()
            .RegisterServices(s => s.AddFellowOakDicom().AddTranscoderManager<FellowOakDicom.Imaging.NativeCodec.NativeTranscoderManager>())
            .RegisterServices(s => s.AddImageManager<ImageSharpImageManager>())
            .SkipValidation()
            .Build();
            if (ModelState.IsValid)
            {
                var dicomPathAndImagesPath = new DicomPathAndImagesPath();
                dicomPathAndImagesPath.DicomPaths = new List<string>();
                dicomPathAndImagesPath.ImagePaths = new List<string>();
                dicomPathAndImagesPath.TypeOfDiagnostics = await _context.TypesOfDiagnostics.FirstOrDefaultAsync(t => t.Id == model.TypeOfDiagnosticsId);
                for (int i = 0; i < model.DicomFiles.Count; i++)
                {
                    var uploadPath = $"{Directory.GetCurrentDirectory()}/wwwroot/images/dicom/{diagnostics.Patient.Code}/diagnostics_id_{model.DiagnosticId}/typeOfDiagnosticsId_{model.TypeOfDiagnosticsId}";
                    Directory.CreateDirectory(uploadPath);
                    using (var fileStream = new FileStream(Path.Combine(uploadPath, model.DicomFiles[i].FileName), FileMode.Create))
                    {
                        await model.DicomFiles[i].CopyToAsync(fileStream);
                    }
                    var typeOfDiagnostics = await _context.TypesOfDiagnostics.FirstOrDefaultAsync(t => t.Id == model.TypeOfDiagnosticsId);
                    dicomPathAndImagesPath.DicomPaths.Add($"/images/dicom/{diagnostics.Patient.Code}/diagnostics_id_{model.DiagnosticId}/typeOfDiagnosticsId_{model.TypeOfDiagnosticsId}/{model.DicomFiles[i].FileName}");
                    //var images = new DicomImage($"{uploadPath}/{model.DicomFiles[i].FileName}");
                    //for (int j = 0; j < images.NumberOfFrames; j++)
                    //{
                    //    images.RenderImage(j).AsSharpImage().SaveAsJpeg($"{uploadPath}/frame{j}_{model.DicomFiles[i].FileName.Substring(0, model.DicomFiles[i].FileName.Length - 4)}.jpeg");
                    //    dicomPathAndImagesPath.ImagePaths.Add($"/images/dicom/{diagnostics.Patient.Code}/diagnostics_id_{model.DiagnosticId}/typeOfDiagnosticsId_{model.TypeOfDiagnosticsId}/frame{j}_{model.DicomFiles[i].FileName.Substring(0, model.DicomFiles[i].FileName.Length - 4)}.jpeg");
                    //}   для формата dcm и функции просмотра
                    dicomPathAndImagesPath.IsDicomDownload = true;
                    if(diagnostics.DicomPathAndImagesPaths.Count > 0)
                    {
                        bool IsNewDicomFile = false;
                        for (int j = 0; j < diagnostics.DicomPathAndImagesPaths.Count; j++)
                        {
                            if (diagnostics.DicomPathAndImagesPaths[j].TypeOfDiagnosticsId == model.TypeOfDiagnosticsId)
                            {
                                diagnostics.DicomPathAndImagesPaths[j] = dicomPathAndImagesPath;
                                break;
                            }
                            if (j == diagnostics.DicomPathAndImagesPaths.Count - 1)
                                IsNewDicomFile = true;
                        }
                        if (IsNewDicomFile)
                            diagnostics.DicomPathAndImagesPaths.Add(dicomPathAndImagesPath);
                    }
                    else
                        diagnostics.DicomPathAndImagesPaths.Add(dicomPathAndImagesPath);
                }
                _context.Diagnostics.Update(diagnostics);
                _context.SaveChanges();
                return RedirectToAction($"DiagnosticsDetails", new {diagnostics.Id});
            }
            return View(model);
        }
        [AcceptVerbs("GET", "POST")]
        public bool DicomCheck(string dicomFiles)
        {
            string[] type = { ".dcm", ".zip", ".rar" };
            foreach (var item in type)
            {
                if (type.Any(t => dicomFiles.ToLower().EndsWith(t)))
                    return true;
            }
            return false;
        }

        public async Task<IActionResult> ShowDicomImages(int diagnosticsId, int typeOfDiagnosticsId)
        {
            if (diagnosticsId == null && typeOfDiagnosticsId == null)
                return BadRequest();
            var diagnostics = await _context.Diagnostics
                .Include(d => d.DicomPathAndImagesPaths).ThenInclude(d => d.TypeOfDiagnostics)
                .FirstOrDefaultAsync(d => d.Id == diagnosticsId);
            if (diagnostics == null)
                return NotFound();
            ViewBag.DiagnosticsId = diagnosticsId;
            return View(diagnostics.DicomPathAndImagesPaths.FirstOrDefault(d => d.TypeOfDiagnosticsId == typeOfDiagnosticsId));
        }
        [AllowAnonymous]
        public async Task<IActionResult> DicomFileDownload(int id)
        {
            if (id == null)
                return BadRequest();
            var dicomPath = await _context.DicomPathAndImagesPaths.FirstOrDefaultAsync(d => d.Id == id);
            if (dicomPath == null)
                return NotFound();
            var splitPath = dicomPath.DicomPaths[0].Split('/');
            var fileName = splitPath[(splitPath.Count() - 1)];
            if (System.IO.File.Exists($"{Directory.GetCurrentDirectory()}/wwwroot{dicomPath.DicomPaths[0]}".Replace("\\", "/")))
                return File(dicomPath.DicomPaths[0], "application/dcm", fileName);
            else
                return NotFound();
        }

        [Authorize(Roles = "x-ray_laboratory_assistant")]
        public async Task<IActionResult> DiagnosticChangeImagesStatus (int? id)
        {
            if (id == null)
                return BadRequest();
            var diagnostic = await _context.Diagnostics.Include(d => d.Laborant).Include(d => d.DiagnosticLog).FirstOrDefaultAsync(d => d.Id == id);
            if (diagnostic == null)
                return NotFound();
            diagnostic.DiagnosticLog.LogList ??= new List<string>();
            if (diagnostic.ImagesStatus == "Available")
            {
                diagnostic.Laborant = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                diagnostic.DiagnosticLog.LogList.Add($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} StartAction {diagnostic.Laborant.Name} {diagnostic.Laborant.Surname}");
                diagnostic.ImagesStatus = "InProcess";
                diagnostic.DiagnosticLog.ResearchStart = DateTime.Now;
            }
            else if (diagnostic.ImagesStatus == "InProcess" && diagnostic.Laborant?.UserName == User.Identity.Name)
            {
                diagnostic.DiagnosticLog.LogList.Add($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} FinishAction {diagnostic.Laborant.Name} {diagnostic.Laborant.Surname}");
                diagnostic.ImagesStatus = "Completed";
                diagnostic.DiagnosticLog.ResearchEnd = DateTime.Now;
            }
            else if (diagnostic.ImagesStatus == "Completed" && diagnostic.Laborant?.UserName == User.Identity.Name)
            {
                diagnostic.DiagnosticLog.LogList.Add($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} ReopenAction {diagnostic.Laborant.Name} {diagnostic.Laborant.Surname}");
                diagnostic.ImagesStatus = "InProcess";
                diagnostic.DiagnosticLog.ResearchEnd = null;
            }
            else
                return RedirectToAction("DiagnosticsDetails", new { id });
            _context.Diagnostics.Update(diagnostic);
            await _context.SaveChangesAsync();
            return RedirectToAction("DiagnosticsDetails", new { id }); 
        }

        [Authorize(Roles = "x-ray_laboratory_assistant")]
        public async Task<IActionResult> RejectDiagnostic (int? id)
        {
            if (id == null)
                return BadRequest();
            var diagnostic = await _context.Diagnostics.Include(d => d.Laborant).Include(d => d.DiagnosticLog).FirstOrDefaultAsync(d => d.Id == id);
            if (diagnostic == null)
                return NotFound();
            if (diagnostic.Laborant?.UserName == User.Identity.Name)
            {
                diagnostic.DiagnosticLog.LogList.Add($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} RejectAction {diagnostic.Laborant.Name} {diagnostic.Laborant.Surname}");
                diagnostic.LaborantId = null;
                diagnostic.ImagesStatus = "Available";
                diagnostic.DiagnosticLog.ResearchStart = null;
                diagnostic.DiagnosticLog.ResearchEnd = null;
                _context.Diagnostics.Update(diagnostic);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("DiagnosticsDetails", new { id });
        }
    }
}
