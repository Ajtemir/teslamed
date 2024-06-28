using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeslaMed.Models;

namespace TeslaMed.Controllers
{
    public class PlaceOfWorksController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<PlaceOfWorksController> _localizer;
        public PlaceOfWorksController(TeslaMedContext context, IStringLocalizer<PlaceOfWorksController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var places = await _context.PlaceOfWorks.OrderBy(d => d.Name).ToListAsync();
            return View(places);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(PlaceOfWork newPlace)
        {
            if (ModelState.IsValid)
            {
                var place = await _context.PlaceOfWorks.AsNoTracking().FirstOrDefaultAsync(p => p.Name.ToLower().Trim() == newPlace.Name.ToLower().Trim());
                if (place != null)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(newPlace);
                }
                await _context.PlaceOfWorks.AddAsync(newPlace);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(newPlace);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();
            var place = await _context.PlaceOfWorks.FirstOrDefaultAsync(p => p.Id == id);
            if (place == null)
                return NotFound();
            return View(place);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PlaceOfWork updPlace)
        {
            if (ModelState.IsValid)
            {
                PlaceOfWork? place = await _context.PlaceOfWorks.AsNoTracking().FirstOrDefaultAsync(p => p.Name.ToLower().Trim() == updPlace.Name.ToLower().Trim());
                if (place != null && place.Id != updPlace.Id)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(updPlace);
                }
                _context.PlaceOfWorks.Update(updPlace);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(updPlace);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            var place = await _context.PlaceOfWorks.FirstOrDefaultAsync(p => p.Id == id);
            if (place == null)
                return NotFound();
            _context.PlaceOfWorks.Remove(place);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
