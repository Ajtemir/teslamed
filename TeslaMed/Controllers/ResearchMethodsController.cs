using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeslaMed.Models;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class ResearchMethodsController : Controller
    {
        private readonly TeslaMedContext _context;

        public ResearchMethodsController (TeslaMedContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rMethods = await _context.ResearchMethods.ToListAsync();
            return View(rMethods);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ResearchMethod newRMethod)
        {
            if (ModelState.IsValid)
            {
                var rMethod = await _context.ResearchMethods.AsNoTracking().FirstOrDefaultAsync(m => m.Name.ToLower().Trim() == newRMethod.Name.ToLower().Trim());
                if (rMethod != null)
                {
                    ModelState.AddModelError("", "Способ исследования с таким названием уже существует");
                    return View(newRMethod);
                }
                await _context.ResearchMethods.AddAsync(newRMethod);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Пожалуйста, заполните все поля!");
            return View(newRMethod);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();
            var rMethod = await _context.ResearchMethods.FirstOrDefaultAsync(m => m.Id == id);
            if (rMethod == null)
                return NotFound();
            return View(rMethod);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ResearchMethod updRMethod)
        {
            if (ModelState.IsValid)
            {
                ResearchMethod? rMethod = await _context.ResearchMethods.AsNoTracking().FirstOrDefaultAsync(m => m.Name.ToLower().Trim() == updRMethod.Name.ToLower().Trim());
                if (rMethod != null && rMethod.Id != updRMethod.Id)
                {
                    ModelState.AddModelError("", "Способ исследования с таким названием уже существует");
                    return View(updRMethod);
                }
                _context.ResearchMethods.Update(updRMethod);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Пожалуйста, заполните все поля!");
            return View(updRMethod);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            var rMetgod = await _context.ResearchMethods.FirstOrDefaultAsync(m => m.Id == id);
            if (rMetgod == null)
                return NotFound();
            _context.ResearchMethods.Remove(rMetgod);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}