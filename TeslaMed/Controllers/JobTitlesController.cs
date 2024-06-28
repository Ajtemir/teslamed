using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeslaMed.Models;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class JobTitlesController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<JobTitlesController> _localizer;
        public JobTitlesController(TeslaMedContext context, IStringLocalizer<JobTitlesController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var jobTitles = await _context.JodTitles.OrderBy(d => d.Title).ToListAsync();
            return View(jobTitles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(JodTitle newJodTitle)
        {
            if (ModelState.IsValid)
            {
                var jobTitle = await _context.JodTitles.AsNoTracking().FirstOrDefaultAsync(j => j.Title.ToLower().Trim() == newJodTitle.Title.ToLower().Trim());
                if (jobTitle != null)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(newJodTitle);
                }
                await _context.JodTitles.AddAsync(newJodTitle);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(newJodTitle);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();
            var jobTitle = await _context.JodTitles.FirstOrDefaultAsync(j => j.Id == id);
            if (jobTitle == null)
                return NotFound();
            return View(jobTitle);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(JodTitle updJobTitle)
        {
            if (ModelState.IsValid)
            {
                JodTitle? jodTitle = await _context.JodTitles.AsNoTracking().FirstOrDefaultAsync(j => j.Title.ToLower().Trim() == updJobTitle.Title.ToLower().Trim());
                if (jodTitle != null && jodTitle.Id != updJobTitle.Id)
                {
                    ModelState.AddModelError("", _localizer["HasInDbError"]);
                    return View(updJobTitle);
                }
                _context.JodTitles.Update(updJobTitle);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", _localizer["Required"]);
            return View(updJobTitle);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            var jodTitle = await _context.JodTitles.FirstOrDefaultAsync(j => j.Id == id);
            if (jodTitle == null)
                return NotFound();
            _context.JodTitles.Remove(jodTitle);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
