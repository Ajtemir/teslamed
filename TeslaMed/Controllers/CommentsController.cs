using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TeslaMed.Models.Repositories;
using TeslaMed.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly IRepository _repo;
        private readonly TeslaMedContext _context;
        private readonly IStringLocalizer<PatientsController> _localizer;

        public CommentsController(TeslaMedContext context, UserManager<User> userManager, IStringLocalizer<PatientsController> localizer, IRepository repo)
        {
            _context = context;
            _localizer = localizer;
            _repo = repo;
        }

        public async Task<IActionResult> CommentsList(int? id)
        {
            if (id == null)
                return BadRequest();
            var diagnostic = await _context.Diagnostics.Include(d => d.Comments).ThenInclude(c => c.Creator).FirstOrDefaultAsync(d => d.Id == id);
            if (diagnostic == null)
                return NotFound();
            ViewBag.DiagnosticId = id;
            return PartialView("~/Views/OperatingActivities/PartialViews/OperationalProcessPartialView.cshtml", diagnostic.Comments);
        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult> Create(int? diagId, string commentText)
        {
            if (diagId == null && commentText == null)
                return Json(400);
            var diagnostic = await _context.Diagnostics.FirstOrDefaultAsync(d => d.Id == diagId);
            if (diagnostic == null)
                return Json(404);
            var comment = new Comment()
            {
                Text = commentText,
                Creator = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name),
                Created = DateTime.Now,
                Diagnostics = diagnostic
            };
            await _context.AddAsync(comment);
            await _context.SaveChangesAsync();
            return Json(200);
        }
    }
}
