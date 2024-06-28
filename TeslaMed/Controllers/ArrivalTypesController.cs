using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeslaMed.Models;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class ArrivalTypesController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<ArrivalTypesController> _localizer;
        public ArrivalTypesController (TeslaMedContext context, IStringLocalizer<ArrivalTypesController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var aTypes = await _context.ArrivalTypes.ToListAsync();
            return View(aTypes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ArrivalType newAType)
        {
            if (ModelState.IsValid)
            {
                var aType = await _context.ArrivalTypes.AsNoTracking().FirstOrDefaultAsync(a => a.Name.ToLower().Trim() == newAType.Name.ToLower().Trim());
                if (aType != null)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(newAType);
                }
                await _context.ArrivalTypes.AddAsync(newAType);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(newAType);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();
            var aType = await _context.ArrivalTypes.FirstOrDefaultAsync(a => a.Id == id);
            if (aType == null)
                return NotFound();
            return View(aType);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ArrivalType updAType)
        {
            if (ModelState.IsValid)
            {
                ArrivalType? aType = await _context.ArrivalTypes.AsNoTracking().FirstOrDefaultAsync(a => a.Name.ToLower().Trim() == updAType.Name.ToLower().Trim());
                if (aType != null && aType.Id != updAType.Id)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(updAType);
                }
                _context.ArrivalTypes.Update(updAType);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(updAType);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            var aType = await _context.ArrivalTypes.FirstOrDefaultAsync(a => a.Id == id);
            if (aType == null)
                return NotFound();
            _context.ArrivalTypes.Remove(aType);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
