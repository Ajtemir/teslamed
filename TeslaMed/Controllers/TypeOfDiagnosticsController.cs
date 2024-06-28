using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NuGet.Packaging;
using TeslaMed.Models;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class TypeOfDiagnosticsController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<TypeOfDiagnosticsController> _localizer;
        public TypeOfDiagnosticsController (TeslaMedContext context, IStringLocalizer<TypeOfDiagnosticsController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var tDiagnostics = await _context.TypesOfDiagnostics.Include(d => d.ResearchMethod).OrderBy(d => d.ResearchMethod.Name).ThenBy(d => d.Name).ToListAsync();
            return View(tDiagnostics);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TypeOfDiagnostics newTDiagnostics)
        {
            if (ModelState.IsValid)
            {
                var researchMethod = await _context.ResearchMethods.FirstOrDefaultAsync(m => m.Id == newTDiagnostics.ResearchMethodId);
                if (researchMethod == null)
                    return BadRequest();
                var tDiagnostics = await _context.TypesOfDiagnostics.AsNoTracking().Include(d => d.ResearchMethod)
                                .FirstOrDefaultAsync(d => (d.Name.ToLower().Trim() == newTDiagnostics.Name.ToLower().Trim() && d.ResearchMethodId == newTDiagnostics.ResearchMethodId));
                if (tDiagnostics != null)
                    return await TODModelError(newTDiagnostics, _localizer["HasInDbError"]);
                await _context.TypesOfDiagnostics.AddAsync(newTDiagnostics);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return await TODModelError(newTDiagnostics,  _localizer["Required"]);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();
            var tDiagnostics = await _context.TypesOfDiagnostics.FirstOrDefaultAsync(d => d.Id == id);
            if (tDiagnostics == null)
                return NotFound();
            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            return View(tDiagnostics);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TypeOfDiagnostics updTDiagnostics)
        {
            if (ModelState.IsValid)
            {
                var researchMethod = await _context.ResearchMethods.FirstOrDefaultAsync(m => m.Id == updTDiagnostics.ResearchMethodId);
                if (researchMethod == null)
                    return BadRequest();
                TypeOfDiagnostics? tDiagnostics = await _context.TypesOfDiagnostics.AsNoTracking()
                                                .FirstOrDefaultAsync(d => (d.Name.ToLower().Trim() == updTDiagnostics.Name.ToLower().Trim() && d.ResearchMethodId == updTDiagnostics.ResearchMethodId));
                if (tDiagnostics != null && tDiagnostics.Id != updTDiagnostics.Id)
                    return await TODModelError(updTDiagnostics, _localizer["HasInDbError"]);
                _context.TypesOfDiagnostics.Update(updTDiagnostics);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return await TODModelError(updTDiagnostics, _localizer["Required"]);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            var tDiagnostics = await _context.TypesOfDiagnostics.FirstOrDefaultAsync(d => d.Id == id);
            if (tDiagnostics == null)
                return NotFound();
            _context.TypesOfDiagnostics.Remove(tDiagnostics);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> TODModelError(TypeOfDiagnostics diagnostic, string error)
        {
            ModelState.AddModelError("", error);
            ViewBag.ResearchMethods = await _context.ResearchMethods.ToListAsync();
            return View(diagnostic);
        }
    }
}
