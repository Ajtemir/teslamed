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
    public class PrivilegesController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<PrivilegesController> _localizer;
        private readonly UserManager<User> _userManager;
        private readonly IRepository _repo;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public PrivilegesController(TeslaMedContext context, IStringLocalizer<PrivilegesController> localizer, UserManager<User> userManager, IRepository repo, IWebHostEnvironment environment)
        {
            _context = context;
            _localizer = localizer;
            _userManager = userManager;
            _repo = repo;
            _hostingEnvironment = environment;
        }
        public IActionResult Index()
        {
            var licenses = _repo.GetAllPrivileges();
            return View(licenses);
        }
        [Authorize(Roles = "admin, manager")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "admin, manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Privileges pr)
        {
            if (!ModelState.IsValid)
            {
                return View(pr);
            }
            await _repo.DbAdd<Privileges>(pr);
            await _repo.DbSave();
            return RedirectToAction("Index", "Privileges");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var faq = _repo.GetPrivileges(id);
            if (faq == null)
            {
                return NotFound();
            }
            return View(faq);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Privileges faq)
        {
            if (id != faq.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(faq);
            }

            _repo.DbUpdate<Privileges>(faq);
            await _repo.DbSave();

            return RedirectToAction("Index", "Privileges");
        }
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> Remove(int id)
        {
            var doc = _repo.GetAllPrivileges().AsQueryable().FirstOrDefault(e => e.Id == id);
            if (doc == null)
            {
                return NotFound();
            }
            _repo.DbRemove(doc);
            await _repo.DbSave();
            return RedirectToAction("Index", "Privileges");
        }
    }
}
