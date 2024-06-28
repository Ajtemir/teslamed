using FellowOakDicom.Imaging;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using System;
using System.ComponentModel;
using System.Data;
using TeslaMed.Models;
using TeslaMed.Models.Repositories;
using TeslaMed.Services;
using TeslaMed.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TeslaMed.Controllers
{
    public class LicensesController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<LicensesController> _localizer;
        private readonly UserManager<User> _userManager;
        private readonly IRepository _repo;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public LicensesController(TeslaMedContext context, IStringLocalizer<LicensesController> localizer, UserManager<User> userManager, IRepository repo, IWebHostEnvironment environment)
        {
            _context = context;
            _localizer = localizer;
            _userManager = userManager;
            _repo = repo;
            _hostingEnvironment = environment;
        }
        public IActionResult Index()
        {
            var licenses = _repo.GetLicences();
            return View(licenses);
        }
        [Authorize(Roles = "admin, manager")]
        public IActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "admin, manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Licences license, List<IFormFile> photos)
        {
            if (ModelState.IsValid)
            {
                if (photos != null && photos.Count > 0)
                {
                    license.Photos = new List<string>();
                    foreach (var photo in photos)
                    {
                        var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                        if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".gif")
                        {
                            var uploadPath = $"{Directory.GetCurrentDirectory()}/wwwroot/images/{Guid.NewGuid()}{fileExtension}";

                            using (var stream = new FileStream(uploadPath, FileMode.Create))
                            {
                                await photo.CopyToAsync(stream);
                            }
                            license.Photos.Add($"/images/{Path.GetFileName(uploadPath)}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Разрешено загружать только файлы изображений (jpg, jpeg, png, gif).");
                            return View(license);
                        }
                    }
                    await _repo.DbAdd(license);
                    await _repo.DbSave();
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Необходимо загрузить хотя бы одно фото.");
                }
            }
            return View(license);
        }


        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> Remove(int id)
        {
            var doc = _repo.GetLicences().AsQueryable().FirstOrDefault(e => e.Id == id);
            if (doc == null)
            {
                return NotFound();
            }
            _repo.DbRemove(doc);
            await _repo.DbSave();
            return RedirectToAction("Index", "Licenses");
        }
        [Authorize(Roles = "admin, manager")]
        [HttpPost]
        public IActionResult DeletePhoto(string photoPath, int licenseId)
        {
            var license = _repo.GetLicences()?.FirstOrDefault(e => e.Id == licenseId);

            if (license != null && license.Photos != null && license.Photos.Contains(photoPath))
            {
                var imagePath = $"{Directory.GetCurrentDirectory()}/wwwroot{photoPath}";
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                license.Photos.Remove(photoPath);
                if (license.Photos.Count == 0)
                {
                    _repo.DbRemove(license);
                    _repo.DbSave();
                    return Json(true);
                }
                _repo.DbUpdate(license);
                _repo.DbSave();
                return Json(true);
            }

            return Json(false);
        }
    }
}
