using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeslaMed.Models;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class TypesOfCashlessPaymentController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<TypesOfCashlessPaymentController> _localizer;
        public TypesOfCashlessPaymentController(TeslaMedContext context, IStringLocalizer<TypesOfCashlessPaymentController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var typesOfCashlessPayment = await _context.TypesOfCachlessPayment.OrderBy(d => d.Name).ToListAsync();
            return View(typesOfCashlessPayment);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TypeOfCashlessPayment newType)
        {
            if (ModelState.IsValid)
            {
                var typeOfCashlessPayment = await _context.TypesOfCachlessPayment.AsNoTracking().FirstOrDefaultAsync(a => a.Name.ToLower().Trim() == newType.Name.ToLower().Trim());
                if (typeOfCashlessPayment != null)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(newType);
                }
                await _context.TypesOfCachlessPayment.AddAsync(newType);
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
            var typeOfCashlessPayment = await _context.TypesOfCachlessPayment.FirstOrDefaultAsync(a => a.Id == id);
            if (typeOfCashlessPayment == null)
                return NotFound();
            return View(typeOfCashlessPayment);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TypeOfCashlessPayment updType)
        {
            if (ModelState.IsValid)
            {
                TypeOfCashlessPayment? typeOfCashlessPayment = await _context.TypesOfCachlessPayment.AsNoTracking().FirstOrDefaultAsync(a => a.Name.ToLower().Trim() == updType.Name.ToLower().Trim());
                if (typeOfCashlessPayment != null && typeOfCashlessPayment.Id != updType.Id)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(updType);
                }
                _context.TypesOfCachlessPayment.Update(updType);
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
            var typeOfCashlessPayment = await _context.TypesOfCachlessPayment.FirstOrDefaultAsync(a => a.Id == id);
            if (typeOfCashlessPayment == null)
                return NotFound();
            _context.TypesOfCachlessPayment.Remove(typeOfCashlessPayment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
