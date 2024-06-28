using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using TeslaMed.Models;
using TeslaMed.Models.Repositories;
using TeslaMed.Services;
using TeslaMed.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TeslaMed.Controllers
{
    public class AboutCompanyController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<AboutCompanyController> _localizer;
        private readonly UserManager<User> _userManager;
        private readonly IRepository _repo;
        public AboutCompanyController(TeslaMedContext context, IStringLocalizer<AboutCompanyController> localizer, UserManager<User> userManager, IRepository repo)
        {
            _context = context;
            _localizer = localizer;
            _userManager = userManager;
            _repo = repo;
        }
        public IActionResult Index()
        {
            var allInfo = _repo.GetAboutCompanies().ToList();
            return View(allInfo);
        }
        [Authorize(Roles = "admin, manager")]
        [HttpGet]
        public IActionResult CreateAbout()
        {
            return View();
        }
        [Authorize(Roles = "admin, manager")]
        [HttpPost]
        public async Task<IActionResult> CreateAbout(AboutCompany doctor, IFormFile file)
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
            await _repo.DbAdd<AboutCompany>(doctor);
            await _repo.DbSave();
            return RedirectToAction("Index", "AboutCompany");
        }
        [Authorize(Roles = "admin, manager")]
        [HttpGet]
        public async Task<IActionResult> EditAbout(int id)
        {
            var aboutCompany = _repo.GetAboutCompany(id);
            if (aboutCompany == null)
            {
                return NotFound();
            }
            return View(aboutCompany);
        }
        [Authorize(Roles = "admin, manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAbout(AboutCompany publication, IFormFile newImage, bool imageChanged)
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
                var existingPublication = _repo.GetAboutCompanies().FirstOrDefault(p => p.Id == publication.Id);

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

            return RedirectToAction("Index", "AboutCompany");
        }

        private bool AboutCompanyExists(int id)
        {
            return (_repo.GetAll<AboutCompany>("AboutCompany")?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> RemoveAbout(int id)
        {
            var doc = _repo.GetAboutCompanies().AsQueryable().FirstOrDefault(e => e.Id == id);
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
            return RedirectToAction("Index", "AboutCompany");
        }
    }
}
