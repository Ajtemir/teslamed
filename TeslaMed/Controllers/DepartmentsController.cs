using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeslaMed.Models;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class DepartmentsController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<DepartmentsController> _localizer;
        public DepartmentsController (TeslaMedContext context, IStringLocalizer<DepartmentsController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
            return View(departments);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Department newDepartment)
        {
            if (ModelState.IsValid)
            {
                var department = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(a => a.Name.ToLower().Trim() == newDepartment.Name.ToLower().Trim());
                if (department != null)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(newDepartment);
                }
                await _context.Departments.AddAsync(newDepartment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(newDepartment);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();
            var department = await _context.Departments.FirstOrDefaultAsync(a => a.Id == id);
            if (department == null)
                return NotFound();
            return View(department);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Department updDepartment)
        {
            if (ModelState.IsValid)
            {
                Department? department = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(a => a.Name.ToLower().Trim() == updDepartment.Name.ToLower().Trim());
                if (department != null && department.Id != updDepartment.Id)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(updDepartment);
                }
                _context.Departments.Update(updDepartment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(updDepartment);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            var department = await _context.Departments.FirstOrDefaultAsync(a => a.Id == id);
            if (department == null)
                return NotFound();
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
