using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeslaMed.Models;
using TeslaMed.Models.Repositories;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class DiscountsController : Controller
    {
        private readonly IRepository _repo;
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<DiscountsController> _localizer;
        public DiscountsController (TeslaMedContext context, IStringLocalizer<DiscountsController> localizer, IRepository repo)
        {
            _context = context;
            _localizer = localizer;
            _repo = repo;
        }

        public async Task<IActionResult> Index ()
        {
            var discounts = await _context.Discounts.ToListAsync();
            return View(discounts);
        }

        [HttpGet]
        public IActionResult Create ()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create (Discount newDiscount)
        {
            if (ModelState.IsValid)
            {
                var discount = await _context.Discounts.AsNoTracking().FirstOrDefaultAsync(d => d.Name.ToLower().Trim() == newDiscount.Name.ToLower().Trim());
                if (discount != null)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(newDiscount);
                }
                await _context.AddAsync(newDiscount);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(newDiscount);
        }

        [HttpGet]
        public async Task<IActionResult> Edit (int? id)
        {
            if (id == null)
                return BadRequest();
            var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.Id == id);
            if (discount == null)
                return NotFound();
            return View(discount);
        }

        [HttpPost]
        public async Task<IActionResult> Edit (Discount updDiscount)
        {
            if (ModelState.IsValid)
            {
                Discount? discount = await _context.Discounts.AsNoTracking().FirstOrDefaultAsync(d => d.Name.ToLower().Trim() == updDiscount.Name.ToLower().Trim());
                if (discount != null && discount.Id != updDiscount.Id) 
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(updDiscount);
                }
                _context.Update(updDiscount);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(updDiscount);
        }

        public async Task<IActionResult> Delete (int? id)
        {
            if (id == null)
                return BadRequest();
            var discount = await _repo.GetAll<Discount>("Discounts").FirstOrDefaultAsync(d => d.Id == id);
            if (discount == null) 
                return NotFound();
            _repo.DbRemove(discount);
            await _repo.DbSave();
            return RedirectToAction("Index");
        }
    }
}
