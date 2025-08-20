using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pharmaflow7.Models;

namespace Pharmaflow7.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<UserRegistrationModel> userRegistrationModels { get; set; }
        public DbSet<LoginViewModel> loginViewModels { get; set; }
        public DbSet<DashboardViewModel> dashboardViewModels { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<VehicleLocation> VehicleLocations { get; set; }
        public DbSet<EmailOtp> EmailOtps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // إعدادات VehicleLocation
            modelBuilder.Entity<VehicleLocation>()
                .Property(v => v.Latitude)
                .HasColumnType("decimal(18,9)")
                .HasPrecision(18, 9);

            modelBuilder.Entity<VehicleLocation>()
                .Property(v => v.Longitude)
                .HasColumnType("decimal(18,9)")
                .HasPrecision(18, 9);

            // إعدادات Shipment decimal properties
            modelBuilder.Entity<Shipment>()
                .Property(s => s.DestinationLatitude)
                .HasColumnType("decimal(18,9)")
                .HasPrecision(18, 9);

            modelBuilder.Entity<Shipment>()
                .Property(s => s.DestinationLongitude)
                .HasColumnType("decimal(18,9)")
                .HasPrecision(18, 9);

            // باقي الإعدادات
            modelBuilder.Entity<Driver>()
                .HasOne(d => d.ApplicationUser)
                .WithOne()
                .HasForeignKey<Driver>(d => d.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Shipment)
                .WithMany()
                .HasForeignKey(n => n.ShipmentId);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Driver)
                .WithMany()
                .HasForeignKey(s => s.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VehicleLocation>()
                .HasOne(v => v.Shipment)
                .WithMany(s => s.VehicleLocations)
                .HasForeignKey(v => v.ShipmentId);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Store)
                .WithMany()
                .HasForeignKey(s => s.StoreId);

            modelBuilder.Entity<Issue>()
                .HasOne(i => i.Company)
                .WithMany()
                .HasForeignKey(i => i.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Issue>()
                .HasOne(i => i.ReportedBy)
                .WithMany()
                .HasForeignKey(i => i.ReportedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Issue>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Company)
                .WithMany()
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Product)
                .WithMany()
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Distributor)
                .WithMany()
                .HasForeignKey(s => s.DistributorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(s => s.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Driver>()
                .HasOne(d => d.Distributor)
                .WithMany()
                .HasForeignKey(d => d.DistributorId)
                .OnDelete(DeleteBehavior.Restrict);

            // EmailOtp configuration
            modelBuilder.Entity<EmailOtp>()
                .HasIndex(e => e.Email);

            modelBuilder.Entity<EmailOtp>()
                .HasIndex(e => new { e.Email, e.OtpCode })
                .IsUnique();
        }
    }
}