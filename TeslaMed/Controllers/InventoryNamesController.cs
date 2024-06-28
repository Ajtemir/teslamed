using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Linq;
using TeslaMed.Models;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class InventoryNamesController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<InventoryNamesController> _localizer;
        public InventoryNamesController(TeslaMedContext context, IStringLocalizer<InventoryNamesController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var inventoryNames = await _context.InventoryNames.ToListAsync();
            return View(inventoryNames);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(InventoryName? newInventoryName, string name, string unit)
        {
            if (name.Contains('!') && unit.Contains('!'))
            {
                name = name.Trim('!').Trim();
                unit = unit.Trim('!').Trim();
                InventoryName inventoryName = new InventoryName()
                {
                    Name = name,
                    Unit = unit
                };
                await _context.InventoryNames.AddAsync(inventoryName);
                await _context.SaveChangesAsync();
                return Json(inventoryName.Id);
            }
            if (ModelState.IsValid)
            {
                var inventoryName = await _context.InventoryNames.AsNoTracking().FirstOrDefaultAsync(i => i.Name.ToLower().Trim() == newInventoryName.Name.ToLower().Trim());
                if (inventoryName != null)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(newInventoryName);
                }
                newInventoryName.Name = newInventoryName.Name.Trim();
                newInventoryName.Unit = newInventoryName.Unit.Trim();
                await _context.InventoryNames.AddAsync(newInventoryName);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(newInventoryName);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();
            var inventoryName = await _context.InventoryNames.FirstOrDefaultAsync(i => i.Id == id);
            if (inventoryName == null)
                return NotFound();
            return View(inventoryName);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(InventoryName updInventoryName)
        {
            if (ModelState.IsValid)
            {
                InventoryName? inventoryName = await _context.InventoryNames.AsNoTracking().FirstOrDefaultAsync(i => i.Name.ToLower().Trim() == updInventoryName.Name.ToLower().Trim());
                if (inventoryName != null && inventoryName.Id != updInventoryName.Id)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(updInventoryName);
                }
                _context.InventoryNames.Update(updInventoryName);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(updInventoryName);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            var inventoryName = await _context.InventoryNames.FirstOrDefaultAsync(i => i.Id == id);
            if (inventoryName == null)
                return NotFound();
            _context.InventoryNames.Remove(inventoryName);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
