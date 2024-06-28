using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TeslaMed.Models
{
    public class AdminInitializer
    {
        private readonly IConfiguration _configuration;

        public AdminInitializer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async System.Threading.Tasks.Task SeedAdminUser(RoleManager<IdentityRole<int>> _roleManager, UserManager<User> _userManager)
        {
            var username = "administrator";
            var adminPassword = "11QWEqwe";
            var roles = new[] { "admin" };
            foreach (var role in roles)
            {
                if (await _roleManager.FindByNameAsync(role) is null)
                    await _roleManager.CreateAsync(new IdentityRole<int>(role));
            }
            if (await _userManager.FindByNameAsync(username) == null)
            {
                User admin = new User
                {
                    UserName = username,
                    Name = "Admin",
                    Surname = "Admin",
                    Address = "None",
                    Departments = new List<Department>() { (new Department { Name = "None" }) },
                    Post = new JodTitle() { Title = "None"},
                    Specializations = new List<Specialization>(),
                    Active = true,
                };
                var result = await _userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(admin, "admin");
                }
            }
        }
    }
}
