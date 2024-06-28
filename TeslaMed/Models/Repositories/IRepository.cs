using Microsoft.EntityFrameworkCore;

namespace TeslaMed.Models.Repositories
{
    public interface IRepository
    {
        DbSet<T> GetAll<T>(string model) where T : class;

        Task DbAdd<T>(T t) where T : class;

        Task DbAddRange(List<object> list);

        void DbRemove<T>(T t) where T : class;

        void DbUpdate<T>(T t) where T : class;

        Task DbSave();
        List<News> GetAllNews();
        List<Link> GetAllLinks();
        News GetOnePublication(int id);
        AboutCompany GetAboutCompany(int id);
        List<MainDoctor> GetMainDoctors();
        Task<List<User>> GetUserByRole(string role);
        Policy GetPolicy(int id);
        History GetHistory(int id);
        FAQ GetFAQ(int id);
        Privileges GetPrivileges(int id);
        Equipment GetEquipment(int id);
        ResearchPreparation GetPreparation(int id);
        List<AboutCompany> GetAboutCompanies();
        List<Equipment> GetEquipments();
        List<FAQ> GetAllFAQ();
        List<History> GetHistories();
        List<Licences> GetLicences();
        List<Policy> GetAllPolicies();
        List<Privileges> GetAllPrivileges();
        List<ResearchPreparation> GetAllPreparations();
    }
}
