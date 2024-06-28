using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeslaMed.Models;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class OperatingCostNamesController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<OperatingCostNamesController> _localizer;
        public OperatingCostNamesController(TeslaMedContext context, IStringLocalizer<OperatingCostNamesController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }
        public async Task<IActionResult> Index()
        {
            var costNames = await _context.OperatingCostNames.ToListAsync();
            return View(costNames);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(OperatingCostName? newCostName, string name, string unit)
        {
            if (name.Contains('!') && unit.Contains('!'))
            {
                name = name.Trim('!').Trim();
                unit = unit.Trim('!').Trim();
                OperatingCostName costName = new OperatingCostName()
                {
                    Name = name,
                    Unit = unit
                };
                await _context.OperatingCostNames.AddAsync(costName);
                await _context.SaveChangesAsync();
                return Json(costName.Id);
            }
            if (ModelState.IsValid)
            {
                var costName = await _context.OperatingCostNames.AsNoTracking().FirstOrDefaultAsync(c => c.Name.ToLower().Trim() == newCostName.Name.ToLower().Trim());
                if (costName != null)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(newCostName);
                }
                newCostName.Name = newCostName.Name.Trim();
                newCostName.Unit = newCostName.Unit.Trim();
                await _context.OperatingCostNames.AddAsync(newCostName);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(newCostName);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();
            var costName = await _context.OperatingCostNames.FirstOrDefaultAsync(c => c.Id == id);
            if (costName == null)
                return NotFound();
            return View(costName);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(OperatingCostName updCostName)
        {
            if (ModelState.IsValid)
            {
                OperatingCostName? costName = await _context.OperatingCostNames.AsNoTracking().FirstOrDefaultAsync(c => c.Name.ToLower().Trim() == updCostName.Name.ToLower().Trim());
                if (costName != null && costName.Id != updCostName.Id)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(updCostName);
                }
                _context.OperatingCostNames.Update(updCostName);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(updCostName);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            var costName = await _context.OperatingCostNames.FirstOrDefaultAsync(c => c.Id == id);
            if (costName == null)
                return NotFound();
            _context.OperatingCostNames.Remove(costName);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
