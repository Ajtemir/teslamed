using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeslaMed.Models;
using TeslaMed.ViewModels;

namespace TeslaMed.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        public readonly TeslaMedContext _context;
        private readonly IStringLocalizer<AccountController> _localizer;
        public AccountController (UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole<int>> roleManager, TeslaMedContext context, IStringLocalizer<AccountController> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _localizer = localizer;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByNameAsync(model.Login);
                if (user != null)
                {
                    if (!user.Active)
                    {
                        ModelState.AddModelError("", _localizer["WrongLogin"]);
                        return View(model);
                    }
                    var result = await _signInManager.PasswordSignInAsync(
                        user,
                        model.Password,
                        false,
                        false
                        );

                    if (result.Succeeded)
                        return Redirect($"~/Patients/Create");
                }
            }
            ModelState.AddModelError("", _localizer["WrongLoginInfo"]);
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [Authorize(Roles = "manager, admin")]
        public async Task<IActionResult> SettingsTeslaMed()
        {
            return _context.Users != null ?
                        View(await _context.Users.ToListAsync()) :
                        Problem("Entity set 'TeslaMedContext.Users'  is null.");
        }
        
        
        public IActionResult Index()
        {
            List<User> users = _context.Users.ToList();
            return View(users);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userViewModel = new CreateUserViewModel
            {
                _Departments = await _context.Departments.ToListAsync(),
                _Specializations = await _context.Specializations.ToListAsync(),
                _Posts = await _context.JodTitles.ToListAsync()
            };
            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.UserName,
                    Name = model.Name,
                    Surname = model.Surname,
                    Patronymic = model.Patronymic,
                    Evaluation = null,
                    Active = model.Active,
                    Address = model.Address,
                    PostId = model.Post,
                    Departments = await _context.Departments.Where(d => model.Departments.Contains(d.Id)).ToListAsync(),
                    Specializations = await _context.Specializations.Where(s => model.Specializations.Contains(s.Id)).ToListAsync(),
                    PhoneNumber = model.PhoneNumber,
                };
                if (model.Role == "registrar")
                {
                    user.Specializations = null;
                }
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    return RedirectToAction("Index", "Account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            model._Posts = await _context.JodTitles.ToListAsync();
            model._Specializations = await _context.Specializations.ToListAsync();
            model._Departments = await _context.Departments.ToListAsync();
            return View(model);
        }

        [AcceptVerbs("GET", "POST")]
        public JsonResult CheckLogin(string userName, int? id)
          {
            userName = userName != null ? userName : " ";
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string noLetter = "!#$%^&*()+=/[]{}';:\",?><`~|\\";
            foreach (var x in userName.ToCharArray())
            {
                if (char.IsLetter(x) && !alphabet.Contains(char.ToUpper(x)))
                    return Json("Логин должен вводиться на английском языке!");
                if (x == ' ')
                    return Json("Логин не должен содержать пробел");
                if (noLetter.Contains(x) && !alphabet.Contains(char.ToUpper(x)))
                    return Json("Логин не должен содержать символы кроме ._-@");
            }
            if ((!_context.Users.Any(b => b.UserName == userName) == false && id == null) || (_userManager.Users.FirstOrDefault(u => u.UserName == userName && u.Id != id)) != null)
                return Json($"Логин '{userName}' уже зарегистрирован!");
            return Json(true);
        }

        [AcceptVerbs("GET", "POST")]
        public JsonResult PasswordCheck(string password)
        {
            password = password != null ? password : " ";
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            foreach (var x in password.ToCharArray())
            {

                if (char.IsLetter(x) && !alphabet.Contains(char.ToUpper(x)))
                    return Json("Пароль должен вводиться на английском языке!");
                if (x == ' ')
                    return Json("Пароль не должен содержать пробел");
            }
            string error = "Пароль должен содержать: ";
            if (!password.Any(char.IsUpper) ||
                !password.Any(char.IsLower) ||
                !password.Any(char.IsNumber) ||
                password.Length < 8)
            {
                if (!password.Any(char.IsUpper))
                    error += "1 букву верхнего регистра, ";
                if (!password.Any(char.IsLower))
                    error += "1 букву нижнего регистра, ";
                if (!password.Any(char.IsNumber))
                    error += "минимум 1 цифру, ";
                if (password.Length < 8)
                    error += "не менее 8 символов";
                return Json(error);
            }
            return Json(true);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            User user = await _context.Users
                .Include(u => u.Specializations)
                .Include(u => u.Departments)
                .Include(u => u.Post)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            ViewBag.User = user;
            ViewBag.Posts = _context.JodTitles.ToList();
            ViewBag.Departments = _context.Departments.ToList();
            ViewBag.Specializations = _context.Specializations.ToList();
            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Name = user.Name,
                Surname = user.Surname,
                Patronymic = user.Patronymic,
                Address = user.Address,
                DepartmentsInUser = user.Departments,
                SpecializationsInUser = user.Specializations,
                PhoneNumber = user.PhoneNumber,
                Post = user.Post,
                Role = await GetUserRole(user),
                _Departments = await _context.Departments.ToListAsync(),
                _Specializations = await _context.Specializations.ToListAsync(),
                _Posts = await _context.JodTitles.ToListAsync()
            };

            return View(model);
        }


        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            User user = await _context.Users.Include(u => u.Departments).Include(u => u.Specializations).FirstOrDefaultAsync(u => u.Id == model.Id);
            if (user == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                user.UserName = model.UserName;
                user.Name = model.Name;
                user.Surname = model.Surname;
                user.Patronymic = model.Patronymic;
                user.Address = model.Address;
                user.Departments = await _context.Departments.Where(d => model.Departments.Contains(d.Id)).ToListAsync();
                user.Specializations = await _context.Specializations.Where(d => model.Specializations.Contains(d.Id)).ToListAsync();
                user.PostId = model.PostId;
                if (model.Role == "registrar")
                {
                    user.Specializations = null;
                }
                user.PhoneNumber = model.PhoneNumber;
                var roles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, roles);
                await _userManager.AddToRoleAsync(user, model.Role);
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _context.SaveChanges();
                    return RedirectToAction("SettingsTeslaMed", "Account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

        private async Task<string> GetUserRole(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault();
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(int? id)
        {
            User user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            ViewBag.User = user;
            ChangePasswordViewModel model = new ChangePasswordViewModel { Id = user.Id };
            return View(model);
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id.ToString());
                if (user == null)
                {
                    return NotFound();
                }

                var passwordValidator = new PasswordValidator<User>();
                var validationResult = await passwordValidator.ValidateAsync(_userManager, user, model.Password);
                if (validationResult.Succeeded)
                {
                    string newPasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);
                    user.PasswordHash = newPasswordHash;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Edit", "Account", new { id = user.Id });
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                else
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            User user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                user.Active = !user.Active;
                await _userManager.UpdateAsync(user);
                return RedirectToAction("Edit", "Account", new { id = user.Id });
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'TeslaMedContext.Users'  is null.");
            }
            var user = await _context.Users.Include(u => u.Departments).Include(u => u.Specializations).FirstAsync(u => u.Id == id); 
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("SettingsTeslaMed", "Account");
        }
        [HttpGet]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
            return LocalRedirect(returnUrl);
        }
    }
}