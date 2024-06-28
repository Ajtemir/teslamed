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
    public class FAQController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<FAQController> _localizer;
        private readonly UserManager<User> _userManager;
        private readonly IRepository _repo;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public FAQController(TeslaMedContext context, IStringLocalizer<FAQController> localizer, UserManager<User> userManager, IRepository repo, IWebHostEnvironment environment)
        {
            _context = context;
            _localizer = localizer;
            _userManager = userManager;
            _repo = repo;
            _hostingEnvironment = environment;
        }
        public IActionResult Index()
        {
            var licenses = _repo.GetAllFAQ().ToList();
            return View(licenses);
        }
        [Authorize(Roles = "admin, manager")]
        [HttpGet]
        public IActionResult CreateFAQ()
        {
            return View();
        }
        [Authorize(Roles = "admin, manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFAQ(FAQ faq)
        {
            if (!ModelState.IsValid)
            {
                return View(faq);
            }

            await _repo.DbAdd<FAQ>(faq);
            await _repo.DbSave();

            return RedirectToAction("Index", "FAQ");
        }
        [HttpGet]
        public async Task<IActionResult> EditFAQ(int id)
        {
            var faq = _repo.GetFAQ(id);
            if (faq == null)
            {
                return NotFound();
            }
            return View(faq);
        }

        [HttpPost]
        public async Task<IActionResult> EditFAQ(int id, FAQ faq)
        {
            if (id != faq.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(faq);
            }

            _repo.DbUpdate<FAQ>(faq);
            await _repo.DbSave();

            return RedirectToAction("Index", "FAQ");
        }
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> RemoveFAQ(int id)
        {
            var doc = _repo.GetAllFAQ().AsQueryable().FirstOrDefault(e => e.Id == id);
            if (doc == null)
            {
                return NotFound();
            }
            _repo.DbRemove(doc);
            await _repo.DbSave();
            return RedirectToAction("Index", "FAQ");
        }
    }
}
