using FellowOakDicom;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeslaMed.Models;
using TeslaMed.ViewModels;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class DoctorsController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IStringLocalizer<DoctorsController> _localizer;
        public DoctorsController(TeslaMedContext context, UserManager<User> userManager, IStringLocalizer<DoctorsController> localizer)
        {
            _context = context;
            _userManager = userManager;
            _localizer = localizer;
        }
        public async Task<IActionResult> ShowListOfDoctors(bool isStaffDoctor)
        {
            var doctorsSpecializations = _context.Users.Include(d => d.Specializations).ToList();
            List<User> doctorResult = new List<User>();
            foreach (var item in doctorsSpecializations)
            {
                if (_userManager.IsInRoleAsync(item, "doctor").Result == true)
                    doctorResult.Add(item);
            }
            FreelanceAndStaffDoctorsListsViewModel allDoctors = new FreelanceAndStaffDoctorsListsViewModel()
            {
                StaffDoctors = doctorResult,
                FreelanceDoctors = _context.Doctors.Include(d => d.PlaceOfWork).ToList()
            };
            ViewBag.IsStaffDoctor = isStaffDoctor;
            return View(allDoctors);
        }

        //Pre-entry
        [HttpGet]
        public async Task<IActionResult> AllEntries(DateTime? filterDate, int? filterResearchMethod, string filterFirstName, string filterLastName)
        {
            DateTime today = DateTime.Now.Date;

            var query = _context.Entries
                .Include(e => e.Patient)
                .Include(e => e.TypeOfDiagnostics)
                .AsQueryable();

            if (!filterDate.HasValue && !filterResearchMethod.HasValue && string.IsNullOrEmpty(filterFirstName) && string.IsNullOrEmpty(filterLastName))
            {
                query = query.Where(e => e.StartTime.Date == today);
            }

            if (filterDate.HasValue)
            {
                query = query.Where(e => e.StartTime.Date == filterDate.Value.Date);
            }

            if (filterResearchMethod.HasValue)
            {
                query = query.Where(e => e.TypeOfDiagnostics.ResearchMethodId == filterResearchMethod.Value);
            }

            if (!string.IsNullOrEmpty(filterFirstName))
            {
                query = query.Where(e => e.Patient.Name.Contains(filterFirstName));
            }

            if (!string.IsNullOrEmpty(filterLastName))
            {
                query = query.Where(e => e.Patient.Surname.Contains(filterLastName));
            }

            var entries = await query.OrderBy(e => e.StartTime).ToListAsync();
            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            User doctor = await _userManager.GetUserAsync(User);
            ViewBag.DoctorId = doctor.Id;
            return View(entries);
        }



        [Authorize(Roles = "doctor, admin, x-ray_laboratory_assistant, registrar, manager")]
        [HttpGet]
        public async Task<IActionResult> ShowEntries(int id, DateTime? filterDate, int? filterResearchMethod, string filterFirstName, string filterLastName)
        {
            User doctor = await _userManager.FindByIdAsync(id.ToString());
            ViewBag.Doctor = doctor;
            if (doctor == null)
            {
                return NotFound();
            }

            var query = _context.Entries
                .Include(e => e.Patient)
                .Include(e => e.TypeOfDiagnostics)
                .Where(e => e.DoctorId == id)
                .AsQueryable();

            if (filterDate.HasValue)
            {
                query = query.Where(e => e.StartTime.Date == filterDate.Value.Date);
            }

            if (filterResearchMethod.HasValue)
            {
                query = query.Where(e => e.TypeOfDiagnostics.ResearchMethodId == filterResearchMethod.Value);
            }

            if (!string.IsNullOrEmpty(filterFirstName))
            {
                query = query.Where(e => e.Patient.Name.Contains(filterFirstName));
            }

            if (!string.IsNullOrEmpty(filterLastName))
            {
                query = query.Where(e => e.Patient.Surname.Contains(filterLastName));
            }

            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            var entries = await query.OrderByDescending(e => e.StartTime).ToListAsync();
            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            return View(entries);
        }

        public IActionResult AppointmentsList(int doctorId, DateTime selectedDate)
        {
            DateTime selectedDateWithoutTime = selectedDate.Date;

            var entries = _context.Entries
                .Where(e => e.DoctorId == doctorId && e.StartTime.Date == selectedDateWithoutTime)
                .ToList();

            var patientIds = entries.Select(e => e.PatientId).Distinct().ToList();
            var patients = _context.Patients.Where(p => patientIds.Contains(p.Id)).ToList();

            var viewModel = new PatientsAndEntriesViewModel
            {
                Entries = entries,
                Patients = patients
            };

            return PartialView("PartialViews/EntriesListPartial", viewModel);
        }


        public async Task<IActionResult> DoctorsList(int? id)
        {
            if (id == null)
                return BadRequest();
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
            if (department == null)
                return NotFound();
            var doctors = await _userManager.GetUsersInRoleAsync("doctor");
            var jma = _context.Users.Include(u => u.Departments).Where(u => (doctors.Any(d => d == u))).ToList();
            DiagnosticsCreateViewModel diagnosticsCreateVM = new DiagnosticsCreateViewModel()
            {
                Doctors = _context.Users.Where(u => (doctors.Any(d => d == u) && u.Departments.Any(d => d.Id == id))).ToList()
            };
            return PartialView("PartialViews/DoctorSelectPartialView", diagnosticsCreateVM);
        }

        [HttpGet]
        public async Task<IActionResult> CreateEntry(int diagnosticsId)
        {
            var diagnostics = await _context.Diagnostics
                .Include(d => d.Patient)
                .Include(d => d.Doctor)
                .Include(d => d.TypesOfDiagnostics)
                .FirstOrDefaultAsync(d => d.Id == diagnosticsId);

            if (diagnostics == null)
            {
                return NotFound();
            }
            ViewBag.Diagnostics = diagnostics;
            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            var viewModel = new PreEntryViewModel
            {
                DiagnosticsId = diagnostics.Id,
                DoctorId = diagnostics.DoctorId,
                TypesOfDiagnostics = diagnostics.TypesOfDiagnostics,
                Date = DateTime.Now,
                SelectedPatientId = diagnostics.PatientId,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEntry(PreEntryViewModel viewModel)
        {
            var diagnostics = await _context.Diagnostics
                .Include(d => d.Patient)
                .Include(d => d.Doctor)
                .Include(d => d.TypesOfDiagnostics)
                .FirstOrDefaultAsync(d => d.Id == viewModel.DiagnosticsId);

            if (ModelState.IsValid)
            {
                var startDate = new DateTime(viewModel.Date.Year, viewModel.Date.Month, viewModel.Date.Day,
                    viewModel.SelectedStartHour, viewModel.SelectedStartMinute, 0);
                var endDate = new DateTime(viewModel.Date.Year, viewModel.Date.Month, viewModel.Date.Day,
                    viewModel.SelectedEndHour, viewModel.SelectedEndMinute, 0);

                if (endDate.Hour < startDate.Hour || (endDate.Hour == startDate.Hour && endDate.Minute <= startDate.Minute))
                {
                    endDate = endDate.AddDays(1);
                }
                bool isTimeSlotAvailable = IsTimeSlotAvailable(viewModel.DoctorId, startDate, endDate);
                if (viewModel.Date.Date == DateTime.Today && viewModel.SelectedStartHour < DateTime.Now.Hour)
                {
                    ModelState.AddModelError(string.Empty, "Нельзя выбрать прошедшее время!");
                    ViewBag.Diagnostics = diagnostics;
                    ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
                    viewModel.TypesOfDiagnostics = await _context.TypesOfDiagnostics.ToListAsync();
                    return View(viewModel);
                }
                if (!isTimeSlotAvailable)
                {
                    ViewBag.Diagnostics = diagnostics;
                    ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
                    viewModel.TypesOfDiagnostics = await _context.TypesOfDiagnostics.ToListAsync();
                    ModelState.AddModelError(string.Empty, "Данное время занято!");
                    return View(viewModel);
                }

                var preEntry = new Pre_entry
                {
                    DiagnosticsId = viewModel.DiagnosticsId,
                    DoctorId = viewModel.DoctorId,
                    PatientId = viewModel.SelectedPatientId,
                    StartTime = startDate,
                    EndTime = endDate,
                    TypeOfDiagnosticsId = viewModel.SelectedTypeOfDiagnosticsId,
                };

                await _context.AddAsync(preEntry);
                await _context.SaveChangesAsync();

                return RedirectToAction("ShowEntries", "Doctors", new { id = viewModel.DoctorId });
            }

            ViewBag.Diagnostics = diagnostics;
            viewModel.TypesOfDiagnostics = await _context.TypesOfDiagnostics.ToListAsync();
            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            return View(viewModel);
        }
        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckDate(DateTime date)
        {
            DateTime currentDate = DateTime.Now.Date;

            if (date.Date < currentDate)
            {
                return Json("Выберите текущую или будущую дату.");
            }
            return Json(true);

        }

        [HttpGet]
        public async Task<IActionResult> EditEntry(int preEntryId)
        {
            var entry = await _context.Entries
                .Include(e => e.Diagnostics)
                .FirstOrDefaultAsync(e => e.Id == preEntryId);
            if (entry == null)
            {
                return NotFound();
            }
            var diagnostics = await _context.Diagnostics
                .Include(d => d.Patient)
                .Include(d => d.Doctor)
                .Include(d => d.TypesOfDiagnostics)
                .FirstOrDefaultAsync(d => d.Id == entry.DiagnosticsId);
            ViewBag.ResearchMethods = _context.ResearchMethods.ToList();
            ViewBag.Diagnostics = diagnostics;
            ViewBag.Type = _context.TypesOfDiagnostics.Find(entry.TypeOfDiagnosticsId);
            var viewModel = new EditEntryViewModel
            {
                EntryId = entry.Id,
                DiagnosticsId = entry.DiagnosticsId,
                SelectedPatientId = entry.PatientId,
                DoctorId = entry.DoctorId,
                Date = entry.StartTime.Date,
                SelectedStartHour = entry.StartTime.Hour,
                SelectedStartMinute = entry.StartTime.Minute,
                SelectedEndHour = entry.EndTime.Hour,
                SelectedEndMinute = entry.EndTime.Minute,
                TypesOfDiagnostics = await _context.TypesOfDiagnostics.ToListAsync(),
                SelectedTypeOfDiagnosticsId = entry.TypeOfDiagnosticsId
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEntry(EditEntryViewModel viewModel)
        {
            var diagnostics = await _context.Diagnostics
                .Include(d => d.Patient)
                .Include(d => d.Doctor)
                .Include(d => d.TypesOfDiagnostics)
                .FirstOrDefaultAsync(d => d.Id == viewModel.DiagnosticsId);

            if (ModelState.IsValid)
            {
                var entry = await _context.Entries.FindAsync(viewModel.EntryId);
                if (entry == null)

                {
                    return NotFound();
                }
                ViewBag.ResearchMethods = _context.ResearchMethods.ToList();
                ViewBag.Type = _context.TypesOfDiagnostics.Find(entry.TypeOfDiagnosticsId);
                ViewBag.Diagnostics = diagnostics;
                var startDate = new DateTime(viewModel.Date.Year, viewModel.Date.Month, viewModel.Date.Day,
                    viewModel.SelectedStartHour, viewModel.SelectedStartMinute, 0);
                var endDate = new DateTime(viewModel.Date.Year, viewModel.Date.Month, viewModel.Date.Day,
                    viewModel.SelectedEndHour, viewModel.SelectedEndMinute, 0);

                bool isTimeSlotAvailable = IsTimeSlotAvailableForEdit(entry.DoctorId, startDate, endDate, viewModel.EntryId);

                if (!isTimeSlotAvailable)

                {
                    ModelState.AddModelError(string.Empty, "Данное время занято!");
                    viewModel.TypesOfDiagnostics = await _context.TypesOfDiagnostics.ToListAsync();
                    ViewBag.ResearchMethods = _context.ResearchMethods.ToList();
                    ViewBag.Type = _context.TypesOfDiagnostics.Find(entry.TypeOfDiagnosticsId);
                    return View(viewModel);
                }
                ViewBag.ResearchMethods = _context.ResearchMethods.ToList();
                ViewBag.Type = _context.TypesOfDiagnostics.Find(entry.TypeOfDiagnosticsId);
                ViewBag.Diagnostics = diagnostics;
                if (endDate.Hour < startDate.Hour || (endDate.Hour == startDate.Hour && endDate.Minute <= startDate.Minute))
                {
                    endDate = endDate.AddDays(1);
                }
                if (viewModel.Date.Date == DateTime.Today && viewModel.SelectedStartHour < DateTime.Now.Hour)
                {
                    ModelState.AddModelError(string.Empty, "Нельзя выбрать прошедшее время!");
                    viewModel.TypesOfDiagnostics = await _context.TypesOfDiagnostics.ToListAsync();
                    ViewBag.ResearchMethods = _context.ResearchMethods.ToList();
                    ViewBag.Type = _context.TypesOfDiagnostics.Find(entry.TypeOfDiagnosticsId);
                    return View(viewModel);
                }
                if (!isTimeSlotAvailable)
                {
                    ModelState.AddModelError(string.Empty, "Данное время занято!");
                    viewModel.TypesOfDiagnostics = await _context.TypesOfDiagnostics.ToListAsync();
                    ViewBag.ResearchMethods = _context.ResearchMethods.ToList();
                    ViewBag.Type = _context.TypesOfDiagnostics.Find(entry.TypeOfDiagnosticsId);
                    return View(viewModel);
                }

                entry.StartTime = startDate;
                entry.EndTime = endDate;
                entry.TypeOfDiagnosticsId = viewModel.SelectedTypeOfDiagnosticsId;

                _context.Update(entry);
                await _context.SaveChangesAsync();

                return RedirectToAction("ShowEntries", "Doctors", new { id = entry.DoctorId });
            }
            ViewBag.Diagnostics = diagnostics;
            ViewBag.ResearchMethods = _context.ResearchMethods.ToList();
            ViewBag.Type = _context.TypesOfDiagnostics.Find(viewModel.SelectedTypeOfDiagnosticsId);
            viewModel.TypesOfDiagnostics = await _context.TypesOfDiagnostics.ToListAsync();
            return View(viewModel);
        }

        private bool IsTimeSlotAvailable(int? doctorId, DateTime startTime, DateTime endTime)
        {
            return !_context.Entries.Any(e =>
                e.DoctorId == doctorId &&
                ((startTime >= e.StartTime && startTime < e.EndTime) ||
                 (endTime > e.StartTime && endTime <= e.EndTime) ||
                 (startTime <= e.StartTime && endTime >= e.EndTime)));
        }

        private bool IsTimeSlotAvailableForEdit(int? doctorId, DateTime startTime, DateTime endTime, int entryId)
        {
            return !_context.Entries.Any(e =>
                e.Id != entryId &&
                e.DoctorId == doctorId &&
                ((startTime >= e.StartTime && startTime < e.EndTime) ||
                 (endTime > e.StartTime && endTime <= e.EndTime) ||
                 (startTime <= e.StartTime && endTime >= e.EndTime)));
        }
        [HttpGet]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            var preEntry = await _context.Entries.FindAsync(id);
            if (preEntry == null)
            {
                return NotFound();
            }
            ViewBag.Patient = await _context.Patients.FindAsync(preEntry.PatientId);
            return View(preEntry);
        }

        [HttpPost, ActionName("DeleteEntry")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEntryConfirmed(int id)
        {
            var preEntry = await _context.Entries.FindAsync(id);
            if (preEntry == null)
            {
                return NotFound();
            }
            _context.Entries.Remove(preEntry);
            await _context.SaveChangesAsync();

            return RedirectToAction("ShowEntries", "Doctors", new { id = preEntry.DoctorId });
        }
        //Freelance doctor
        public async Task<IActionResult> Index()
        {
            var freelanceDoctors = await _context.Doctors.Include(d => d.PlaceOfWork).ToListAsync();
            return View(freelanceDoctors);
        }

        [Authorize(Roles = "manager, admin")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.PlacesOfWork = _context.PlaceOfWorks.ToList();
            return View();
        }

        [Authorize(Roles = "manager, admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Doctor newDoctor)
        {
            if (ModelState.IsValid)
            {
                var placeOfWork = await _context.PlaceOfWorks.AsNoTracking().FirstOrDefaultAsync(p => p.Name.ToLower().Trim() == newDoctor.PlaceOfWork.Name.ToLower().Trim());
                if (placeOfWork != null)
                {
                    newDoctor.PlaceOfWorkId = placeOfWork.Id;
                    newDoctor.PlaceOfWork = null;                    
                }
                bool hasDuplicate = _context.Doctors.Any(d =>
                    d.Name.ToLower().Trim() == newDoctor.Name.ToLower().Trim() &&
                    d.Surname.ToLower().Trim() == newDoctor.Surname.ToLower().Trim() &&
                    ((d.Patronymic.ToLower().Trim()) == (newDoctor.Patronymic != null ? newDoctor.Patronymic.ToLower().Trim() : null) || d.Patronymic == null) &&
                    d.PlaceOfWorkId == newDoctor.PlaceOfWorkId
                );

                if (hasDuplicate)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(newDoctor);
                }
                newDoctor.Manager = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                await _context.AddAsync(newDoctor);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(newDoctor);
        }

        [Authorize(Roles = "manager, admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();

            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null)
                return NotFound();

            ViewBag.PlacesOfWork = _context.PlaceOfWorks.ToList();
            return View(doctor);
        }

        [Authorize(Roles = "manager, admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(Doctor updatedDoctor)
        {
            if (ModelState.IsValid)
            {
                var placeOfWork = await _context.PlaceOfWorks.AsNoTracking().FirstOrDefaultAsync(p => p.Name.ToLower().Trim() == updatedDoctor.PlaceOfWork.Name.ToLower().Trim());
                if (placeOfWork != null)
                {
                    updatedDoctor.PlaceOfWorkId = placeOfWork.Id;
                    updatedDoctor.PlaceOfWork = null;
                }
                var duplicateDoctor = await _context.Doctors.FirstOrDefaultAsync(d =>
                    d.Id != updatedDoctor.Id &&
                    d.Name.ToLower().Trim() == updatedDoctor.Name.ToLower().Trim() &&
                    d.Surname.ToLower().Trim() == updatedDoctor.Surname.ToLower().Trim() &&
                    (d.Patronymic == null || d.Patronymic.ToLower().Trim() == (updatedDoctor.Patronymic != null ? updatedDoctor.Patronymic.ToLower().Trim() : null)) &&
                    d.PlaceOfWorkId == updatedDoctor.PlaceOfWorkId
                );

                if (duplicateDoctor != null)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(updatedDoctor);
                }

                _context.Doctors.Update(updatedDoctor);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(updatedDoctor);
        }

        [Authorize(Roles = "manager, admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null)
                return NotFound();
            _context.Remove(doctor);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
