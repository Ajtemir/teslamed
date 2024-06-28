using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Data;
using TeslaMed.Models;
using TeslaMed.ViewModels;

namespace TeslaMed.Controllers
{
    [Authorize(Roles = "admin")]
    public class SpecializationsController : Controller
    {
        public readonly TeslaMedContext _context;
        private readonly IStringLocalizer<DepartmentsController> _localizer;
        public SpecializationsController(TeslaMedContext context, IStringLocalizer<DepartmentsController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var specializations = await _context.Specializations.ToListAsync();
            return View(specializations);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Specialization specialization)
        {
            if (ModelState.IsValid)
            {
                var sp = await _context.Specializations.AsNoTracking().FirstOrDefaultAsync(d => d.Name.ToLower().Trim() == specialization.Name.ToLower().Trim());
                if (sp != null)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(specialization);
                }
                await _context.AddAsync(specialization);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(specialization);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();
            var sp = await _context.Specializations.FirstOrDefaultAsync(d => d.Id == id);
            if (sp == null)
                return NotFound();
            return View(sp);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Specialization upSp)
        {
            if (ModelState.IsValid)
            {
                Specialization? sp = await _context.Specializations.AsNoTracking().FirstOrDefaultAsync(d => d.Name.ToLower().Trim() == upSp.Name.ToLower().Trim());
                if (sp != null && sp.Id != upSp.Id)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(upSp);
                }
                _context.Update(upSp);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(upSp);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            var sp = await _context.Specializations.FirstOrDefaultAsync(d => d.Id == id);
            if (sp == null)
                return NotFound();
            _context.Remove(sp);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
