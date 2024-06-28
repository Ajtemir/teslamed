using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TeslaMed.Models;
using TeslaMed.Models.Repositories;
using Rotativa.AspNetCore;
using TeslaMed.Services;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;

namespace TeslaMed
{
    public class Program
    {
        private static IWebHostEnvironment env;
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.

            string connection = builder.Configuration["ConnectionStrings:DefaultConnection"];
            var footerConfig = builder.Configuration.GetSection("FooterConfiguration").Get<FooterConfiguration>();

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("ru"),
                    new CultureInfo("ky")
                };

                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
            builder.Services.AddDbContext<TeslaMedContext>(options => options.UseNpgsql(connection))
                .AddIdentity<User, IdentityRole<int>>(options =>
                {
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 8;
                })
                .AddEntityFrameworkStores<TeslaMedContext>();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/Account/Login");
                });
            builder.Services.AddScoped<IRepository, MyRepository>();
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            builder.Services.AddControllersWithViews().AddDataAnnotationsLocalization().AddViewLocalization();
            builder.Services.AddSingleton(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic }));
            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
            });
            builder.Services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
                x.MultipartHeadersLengthLimit = int.MaxValue;
            });
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var app = builder.Build();
            var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var adminInitializer = new AdminInitializer(app.Configuration);
                await adminInitializer.SeedAdminUser(roleManager, userManager);
                var roles = new[] { "registrar", "x-ray_laboratory_assistant", "manager", "doctor" };
                foreach (var role in roles)
                {
                    if (await roleManager.FindByNameAsync(role) is null)
                        await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }
            using (var scope = scopeFactory.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var context = services.GetRequiredService<TeslaMedContext>();
                    context.Database.EnsureCreated();

                    if (!context.FooterSettings.Any())
                    {
                        context.FooterSettings.Add(new FooterConfiguration
                        {
                            Phone1 = "+996 502 56 34 00",
                            Phone2 = "+996 552 56 34 00",
                            Phone3 = "+996 312 56 34 00",
                            Email = "teslamedworkstation@gmail.com",
                            Address = "Кыргызстан, г. Бишкек, улица Горького, 95"
                        });

                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
            IWebHostEnvironment env = app.Environment;
            //RotativaConfiguration.Setup((Microsoft.AspNetCore.Hosting.IHostingEnvironment)env, @"../wkhtmltopdf\bin");
            RotativaConfiguration.Setup(env.WebRootPath, "/usr/bin/");


            //Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            var supportedCultures = new[]
            {
                new CultureInfo("ru"),
                new CultureInfo("ky")
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("ru"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
                RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider()
                }
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=News}/{action=MainPage}/");

            app.Run();
        }
    }
}
