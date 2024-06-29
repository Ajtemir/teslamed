using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace TeslaMed.Models
{
    public class TeslaMedContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Diagnostics> Diagnostics { get; set; }
        public DbSet<TypeOfDiagnostics> TypesOfDiagnostics { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<ArrivalType> ArrivalTypes { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Pre_entry> Entries { get; set; }
        public DbSet<JodTitle> JodTitles { get; set; }
        public DbSet<ResearchMethod> ResearchMethods { get; set; }
        public DbSet<DiagnosticLog> DiagnosticLogs { get; set; }
        public DbSet<TypeOfCashlessPayment> TypesOfCachlessPayment { get; set; }
        public DbSet<Revision> Revisions { get; set; }
        public DbSet<InventoryName> InventoryNames { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<FlowAccounting> FlowAccountings { get; set; }
        public DbSet<DicomPathAndImagesPath> DicomPathAndImagesPaths { get; set; }
        public DbSet<OperatingCostName> OperatingCostNames { get; set; }
        public DbSet<TypeOfCosts> TypesOfCosts { get; set; }
        public DbSet<OperatingCost> OperatingCosts { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Link> Links { get; set; }
        public DbSet<MainDoctor> MainDoctors { get; set; }
        public DbSet<PlaceOfWork> PlaceOfWorks { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Privileges> Privileges { get; set; }
        public DbSet<FAQ> FAQ { get; set; }
        public DbSet<AboutCompany> AboutCompany { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<Policy> Policy { get; set; }
        public DbSet<Licences> Licences { get; set; }
        public DbSet<ResearchPreparation> ResearchPreparations { get; set; }
        public DbSet<FooterConfiguration> FooterSettings { get; set; }
        public TeslaMedContext(DbContextOptions<TeslaMedContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Diagnostics>()
                .HasMany(d => d.TypesOfDiagnostics)
                .WithMany(t => t.Diagnostics);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Specializations)
                .WithMany(s => s.Users);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Departments)
                .WithMany(d => d.Users);
            // modelBuilder.Entity<Diagnostics>()
            //     .HasMany(d => d.DicomPathAndImagesPaths)
            //     .WithOne();
            modelBuilder.Entity<Diagnostics>()
                .HasMany(d => d.Comments)
                .WithOne(d => d.Diagnostics);
            modelBuilder.Entity<News>()
                .HasMany(n => n.Links)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.PlaceOfWork)
                .WithMany(p => p.Doctors)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }

}
