using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Generic;

namespace TeslaMed.Models.Repositories
{
    public class MyRepository : IRepository
    {
        private readonly TeslaMedContext _context;
        private readonly UserManager<User> _userManager;

        public MyRepository(TeslaMedContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public List<News> GetAllNews()
        {
            return _context.News
                .OrderByDescending(n => n.Date)
                .Include(n => n.Links).ToList();
        }
        public List<AboutCompany> GetAboutCompanies()
        {
            return _context.AboutCompany.ToList();
        }
        public List<Equipment> GetEquipments()
        {
            return _context.Equipments.ToList();
        }
        public List<FAQ> GetAllFAQ()
        {
            return _context.FAQ.ToList();
        }
        public List<History> GetHistories()
        {
            return _context.History.ToList();
        }
        public List<Licences> GetLicences()
        {
            return _context.Licences.ToList();
        }
        public List<Policy> GetAllPolicies()
        {
            return _context.Policy.ToList();
        }
        public List<Privileges> GetAllPrivileges()
        {
            return _context.Privileges.ToList();
        }
        public List<ResearchPreparation> GetAllPreparations()
        {
            return _context.ResearchPreparations.ToList();
        }
        
        public List<Link> GetAllLinks()
        {
            return _context.Links.ToList();
        }
        public List<MainDoctor> GetMainDoctors()
        {
            return _context.MainDoctors.ToList();
        }
        public News GetOnePublication(int id)
        {
            var publication = _context.News.FirstOrDefault(x => x.Id == id);
            return publication;
        }
        public Link GetOneLink(int id)
        {
            var link = _context.Links.FirstOrDefault(x => x.Id == id);
            return link;
        }
        public AboutCompany GetAboutCompany(int id)
        {
            var link = _context.AboutCompany.FirstOrDefault(x => x.Id == id);
            return link;
        }
        public Policy GetPolicy(int id)
        {
            var link = _context.Policy.FirstOrDefault(x => x.Id == id);
            return link;
        }
        public History GetHistory(int id)
        {
            var link = _context.History.FirstOrDefault(x => x.Id == id);
            return link;
        }
        public FAQ GetFAQ(int id)
        {
            var link = _context.FAQ.FirstOrDefault(x => x.Id == id);
            return link;
        }
        public Equipment GetEquipment(int id)
        {
            var link = _context.Equipments.FirstOrDefault(x => x.Id == id);
            return link;
        }
        public Privileges GetPrivileges(int id)
        {
            var link = _context.Privileges.FirstOrDefault(x => x.Id == id);
            return link;
        }
        public ResearchPreparation GetPreparation(int id)
        {
            var link = _context.ResearchPreparations.FirstOrDefault(x => x.Id == id);
            return link;
        }
        public DbSet<T> GetAll<T>(string table) where T : class
        {
            return (DbSet<T>)_context.GetType().GetProperties().FirstOrDefault(x => x.Name == table).GetValue(_context, null);
        }

        public async Task DbSave()
        {
            await _context.SaveChangesAsync();
        }

        public void DbUpdate<T>(T t) where T : class
        {
            _context.Update(t);
        }

        public async Task DbAdd<T>(T t) where T : class
        {
            await _context.AddAsync(t);
        }

        public async Task DbAddRange(List<object> list)
        {
            await _context.AddRangeAsync(list);
        }

        public void DbRemove<T>(T t) where T : class
        {
            _context.Remove(t);
        }

        public async Task<List<User>> GetUserByRole(string role)
        {
            return (List<User>)await _userManager.GetUsersInRoleAsync(role);
        }
    }
}
