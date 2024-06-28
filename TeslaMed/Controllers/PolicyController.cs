using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TeslaMed.Models.Repositories;
using TeslaMed.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace TeslaMed.Controllers
{
    public class PolicyController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<PolicyController> _localizer;
        private readonly UserManager<User> _userManager;
        private readonly IRepository _repo;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public PolicyController(TeslaMedContext context, IStringLocalizer<PolicyController> localizer, UserManager<User> userManager, IRepository repo, IWebHostEnvironment environment)
        {
            _context = context;
            _localizer = localizer;
            _userManager = userManager;
            _repo = repo;
            _hostingEnvironment = environment;
        }
        public IActionResult Index()
        {
            var allInfo = _repo.GetAllPolicies();
            return View(allInfo);
        }
        [Authorize(Roles = "admin, manager")]
        [HttpGet]
        public IActionResult CreatePolicy()
        {
            return View();
        }
        [Authorize(Roles = "admin, manager")]
        [HttpPost]
        public async Task<IActionResult> CreatePolicy(Policy doctor, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var uploadPath = $"{Directory.GetCurrentDirectory()}/wwwroot/images/{file.FileName}";
                using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                doctor.Image = $"/images/{file.FileName}";
            }
            await _repo.DbAdd<Policy>(doctor);
            await _repo.DbSave();
            return RedirectToAction("Index", "Policy");
        }
        [Authorize(Roles = "admin, manager")]
        [HttpGet]
        public async Task<IActionResult> EditPolicy(int id)
        {
            var aboutCompany = _repo.GetPolicy(id);
            if (aboutCompany == null)
            {
                return NotFound();
            }
            return View(aboutCompany);
        }
        [Authorize(Roles = "admin, manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPolicy(Policy publication, IFormFile newImage, bool imageChanged)
        {
            if (publication == null)
            {
                return View(publication);
            }

            if (newImage != null && newImage.Length > 0)
            {
                var uploadPath = $"{Directory.GetCurrentDirectory()}/wwwroot/images/{newImage.FileName}";
                using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                {
                    await newImage.CopyToAsync(fileStream);
                }
                publication.Image = $"/images/{newImage.FileName}";
            }

            try
            {
                var existingPublication = _repo.GetAllPolicies()
                    .FirstOrDefault(p => p.Id == publication.Id);

                if (existingPublication == null)
                {
                    return NotFound();
                }
                bool publicationChanged = existingPublication.Text != publication.Text ||
                                 existingPublication.Image != publication.Image;

                existingPublication.Text = publication.Text;
                existingPublication.Image = publication.Image;
                _repo.DbUpdate(existingPublication);
                await _repo.DbSave();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AboutCompanyExists(publication.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("Index", "Policy");
        }

        private bool AboutCompanyExists(int id)
        {
            return (_repo.GetAllPolicies()?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> RemovePolicy(int id)
        {
            var doc = _repo.GetAllPolicies().AsQueryable().FirstOrDefault(e => e.Id == id);
            if (doc == null)
            {
                return NotFound();
            }
            var imagePath = $"{Directory.GetCurrentDirectory()}/wwwroot{doc.Image}";
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            _repo.DbRemove(doc);
            await _repo.DbSave();
            return RedirectToAction("Index", "Policy");
        }
    }
}
