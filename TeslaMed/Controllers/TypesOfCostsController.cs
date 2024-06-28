using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeslaMed.Models;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class TypesOfCostsController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<TypesOfCostsController> _localizer;
        public TypesOfCostsController(TeslaMedContext context, IStringLocalizer<TypesOfCostsController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }
        public async Task<IActionResult> Index()
        {
            var costTypes = await _context.TypesOfCosts.ToListAsync();
            return View(costTypes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TypeOfCosts newType, string name)
        {
            if (name.Contains('!'))
            {
                name = name.Trim('!').Trim();
                TypeOfCosts costType = new TypeOfCosts()
                {
                    Name = name
                };
                await _context.TypesOfCosts.AddAsync(costType);
                await _context.SaveChangesAsync();
                return Json(costType.Id);
            }
            if (ModelState.IsValid)
            {
                var costType = await _context.TypesOfCosts.AsNoTracking().FirstOrDefaultAsync(a => a.Name.ToLower().Trim() == newType.Name.ToLower().Trim());
                if (costType != null)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(newType);
                }
                newType.Name = newType.Name.Trim();
                await _context.TypesOfCosts.AddAsync(newType);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(newType);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();
            var costType = await _context.TypesOfCosts.FirstOrDefaultAsync(t => t.Id == id);
            if (costType == null)
                return NotFound();
            return View(costType);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TypeOfCosts updType)
        {
            if (ModelState.IsValid)
            {
                TypeOfCosts? costType = await _context.TypesOfCosts.AsNoTracking().FirstOrDefaultAsync(a => a.Name.ToLower().Trim() == updType.Name.ToLower().Trim());
                if (costType != null && costType.Id != updType.Id)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(updType);
                }
                _context.TypesOfCosts.Update(updType);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(updType);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            var costType = await _context.TypesOfCosts.FirstOrDefaultAsync(t => t.Id == id);
            if (costType == null)
                return NotFound();
            _context.TypesOfCosts.Remove(costType);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}
