using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using System.Data;
using TeslaMed.Models;
using TeslaMed.Models.Repositories;
using TeslaMed.Services;
using TeslaMed.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace TeslaMed.Controllers
{
    public class NewsController : Controller
    {
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<NewsController> _localizer;
        private readonly UserManager<User> _userManager;
        private readonly IRepository _repo;
        private readonly IConfiguration _configuration;
        public NewsController(TeslaMedContext context, IStringLocalizer<NewsController> localizer, UserManager<User> userManager, IRepository repo, IConfiguration configuration)
        {
            _context = context;
            _localizer = localizer;
            _userManager = userManager;
            _repo = repo;
            _configuration = configuration;
        }
        public IActionResult MainPage()
        {
            if (HttpContext.Request.Host.Host == "mis.teslamed.kg")
            {
                return RedirectToAction("Create", "Patients");
            }
            if (HttpContext.Request.Host.Host == "result.teslamed.kg")
            {
                return RedirectToAction("GetResults", "News");
            }
            var latestNews = _repo.GetAllNews()
                .AsQueryable()
                .OrderByDescending(n => n.Date)
                .Include(n => n.Links)
                .FirstOrDefault();
            var mainDoctors = _repo.GetMainDoctors();

            MainPageViewModel viewModel = new MainPageViewModel()
            {
                Publication = latestNews,
                MainDoctors = mainDoctors,
                Phone1 = _repo.GetAll<FooterConfiguration>("FooterSettings").FirstOrDefault()?.Phone1,
                Phone2 = _repo.GetAll<FooterConfiguration>("FooterSettings").FirstOrDefault()?.Phone2,
                Phone3 = _repo.GetAll<FooterConfiguration>("FooterSettings").FirstOrDefault()?.Phone3,
                Email = _repo.GetAll<FooterConfiguration>("FooterSettings").FirstOrDefault()?.Email,
                Address = _repo.GetAll<FooterConfiguration>("FooterSettings").FirstOrDefault()?.Address
            };
            return View(viewModel);
        }

        public IActionResult Index(DateTime? creationDateFilter, DateTime? redactionDateTimeFilter, string titleFilter, bool resetFilters = false, int page = 1)
        {
            int pageSize = 5;

            var news = _repo.GetAllNews()
                .AsQueryable()
                .Include(n => n.Links)
                .OrderByDescending(n => n.Date)
                .AsQueryable();

            if (resetFilters)
            {
                creationDateFilter = null;
                redactionDateTimeFilter = null;
            }

            if (creationDateFilter.HasValue)
            {
                news = news.Where(e => e.Date.Date == creationDateFilter.Value.Date);
            }

            if (redactionDateTimeFilter.HasValue)
            {
                news = news.Where(e => e.EditDate.HasValue && e.EditDate.Value.Date == redactionDateTimeFilter.Value.Date);
            }

            if (!string.IsNullOrEmpty(titleFilter))
            {
                news = news.Where(n => n.Title.ToLower().Trim().Contains(titleFilter.ToLower().Trim()));
            }

            var totalCount = news.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var filteredNews = news
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.PageNumber = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.titleFilter = titleFilter;
            ViewBag.creationDateFilter = creationDateFilter;
            ViewBag.redactionDateTimeFilter = redactionDateTimeFilter;

            return View(filteredNews);
        }
        [Authorize(Roles = "admin, manager")]
        [HttpGet]
        public ActionResult Create()
        {
            var news = new News();
            news.Links.Add(new Link());
            return View(news);
        }
        [Authorize(Roles = "admin, manager")]
        [HttpPost]
        public async Task<IActionResult> Create(News news, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var uploadPath = $"{Directory.GetCurrentDirectory()}/wwwroot/images/{file.FileName}";
                using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                news.Image = $"/images/{file.FileName}";
            }
            var validLinks = new List<Link>();

            foreach (var link in news.Links)
            {
                if (!string.IsNullOrWhiteSpace(link.Title) && !string.IsNullOrWhiteSpace(link.Url))
                {
                    validLinks.Add(link);
                }
            }

            if (validLinks.Count > 0)
            {
                news.Links = validLinks;
                await _repo.DbAdd<News>(news);
                await _repo.DbSave();
                return RedirectToAction("Index", "News");
            }
            else
            {
                news.Links = null;
                await _repo.DbAdd<News>(news);
                await _repo.DbSave();
                return RedirectToAction("Index", "News");
            }
        }
        [Authorize(Roles = "admin, manager")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _repo.GetAllNews() == null)
            {
                return NotFound();
            }

            var publication = _repo.GetAllNews()
            .AsQueryable()
            .Include(p => p.Links)
            .FirstOrDefault(p => p.Id == id);
            if (publication == null)
            {
                return NotFound();
            }
            return View(publication);
        }
        [Authorize(Roles = "admin, manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(News publication, IFormFile newImage, bool imageChanged)
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
                var existingPublication = _repo.GetAllNews()
                    .AsQueryable()
                    .Include(p => p.Links)
                    .FirstOrDefault(p => p.Id == publication.Id);

                if (existingPublication == null)
                {
                    return NotFound();
                }
                bool publicationChanged = existingPublication.Title != publication.Title ||
                                 existingPublication.Description != publication.Description ||
                                 existingPublication.Image != publication.Image ||
                                 existingPublication.Links.Count != publication.Links.Count;

                if (publicationChanged)
                {
                    existingPublication.EditDate = DateTime.Now;
                }

                existingPublication.Links.Clear();
                if (publication.Links != null)
                {
                    foreach (var link in publication.Links)
                    {
                        existingPublication.Links.Add(link);
                    }
                }
                existingPublication.Title = publication.Title;
                existingPublication.Description = publication.Description;
                existingPublication.Image = publication.Image;
                _repo.DbUpdate(existingPublication);
                await _repo.DbSave();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PublicationExists(publication.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("Index");
        }

        public bool PublicationExists(int id)
        {
            return (_repo.GetAllNews()?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var news = _repo.GetAllNews().AsQueryable().FirstOrDefault(e => e.Id == id);
            if (news == null)
            {
                return NotFound();
            }
            var imagePath = $"{Directory.GetCurrentDirectory()}/wwwroot{news.Image}";
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            _repo.DbRemove(news);
            await _repo.DbSave();
            return RedirectToAction("Index", "News");
        }
        [Authorize(Roles = "admin, manager")]
        [HttpPost]
        public IActionResult DeleteLink(int id, int newsId)
        {
            var link = _repo.GetAllLinks().FirstOrDefault(l => l.Id == id);
            if (link == null)
            {
                return NotFound();
            }
            _repo.DbRemove(link);
            _repo.DbSave();
            return Json(new { success = true, redirectTo = Url.Action("Edit", "News", new { id = newsId }) });
        }
        public IActionResult SendMessage(string name, string phoneNumber, string complaints)
        {
            string message = $"Имя: {name}.<br />Номер телефона: {phoneNumber}<br />Жалобы: {complaints}";
            EmailService emailService = new EmailService();
            emailService.SendEmailAsync("teslamedworkstation@gmail.com", "Запись на приём", message);
            return RedirectToAction("MainPage", "News");
        }
        [Authorize(Roles = "admin, manager")]
        [HttpGet]
        public ActionResult AddMainDoctor()
        {
            return View();
        }
        [Authorize(Roles = "admin, manager")]
        [HttpPost]
        public async Task<IActionResult> AddMainDoctor(MainDoctor doctor, IFormFile file)
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
            await _repo.DbAdd<MainDoctor>(doctor);
            await _repo.DbSave();
            return RedirectToAction("MainPage", "News");
        }
        [Authorize(Roles = "admin, manager")]
        public async Task<IActionResult> RemoveMainDoctor(int id)
        {
            var doc = _repo.GetMainDoctors().AsQueryable().FirstOrDefault(e => e.Id == id);
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
            return RedirectToAction("MainPage", "News");
        }
        [Authorize(Roles = "admin, manager")]
        public IActionResult EditFooter()
        {
            var footerSettings = _context.FooterSettings.FirstOrDefault();

            if (footerSettings == null)
            {
                footerSettings = new FooterConfiguration();
                _context.FooterSettings.Add(footerSettings);
                _context.SaveChanges();
            }

            var viewModel = new FooterSettingsViewModel
            {
                Phone1 = footerSettings.Phone1,
                Phone2 = footerSettings.Phone2,
                Phone3 = footerSettings.Phone3,
                Email = footerSettings.Email,
                Address = footerSettings.Address
            };

            return View(viewModel);
        }

        [Authorize(Roles = "admin, manager")]
        [HttpPost]
        public IActionResult SaveFooter(FooterConfiguration model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditFooter", model);
            }

            var footerSettings = _context.FooterSettings.FirstOrDefault();

            if (footerSettings == null)
            {
                return NotFound();
            }
            footerSettings.Phone1 = model.Phone1;
            footerSettings.Phone2 = model.Phone2;
            footerSettings.Phone3 = model.Phone3;
            footerSettings.Email = model.Email;
            footerSettings.Address = model.Address;
            _context.SaveChanges();
            return RedirectToAction("MainPage");
        }
        [HttpGet]
        public IActionResult GetResults()
        {
            return View(new Diagnostics { Id = -1 });
        }
        [HttpPost]
        public IActionResult GetResults(string resultsCode)
        {
            var diagnostic = _context.Diagnostics.Include(d => d.Patient).Include(d => d.TypesOfDiagnostics).ThenInclude(d => d.ResearchMethod).Include(d => d.DicomPathAndImagesPaths).FirstOrDefault(d => d.Patient.Code + d.Id == resultsCode);
            return View(diagnostic);
        }
    }
}