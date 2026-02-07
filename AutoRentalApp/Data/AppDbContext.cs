using AutoRentalApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoRentalApp.Data
{
    public class AppDbContext : DbContext
    {
        // Все таблицы базы данных
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<CarStatus> CarStatuses { get; set; }
        public DbSet<RentalContract> RentalContracts { get; set; }
        public DbSet<ContractStatus> ContractStatuses { get; set; }
        public DbSet<CarInspection> CarInspections { get; set; }
        public DbSet<AdditionalService> AdditionalServices { get; set; }
        public DbSet<ContractService> ContractServices { get; set; }

        private readonly string _connectionString;

        public AppDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка связей между таблицами

            // Связь User  Role
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleID)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь Client  User
            modelBuilder.Entity<Client>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь Employee  User
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь Car  CarStatus
            modelBuilder.Entity<Car>()
                .HasOne(c => c.CarStatus)
                .WithMany()
                .HasForeignKey(c => c.CarStatusID)
                .OnDelete(DeleteBehavior.Restrict);

            // Связи для RentalContract
            modelBuilder.Entity<RentalContract>()
                .HasOne(rc => rc.Client)
                .WithMany()
                .HasForeignKey(rc => rc.ClientID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RentalContract>()
                .HasOne(rc => rc.Car)
                .WithMany()
                .HasForeignKey(rc => rc.CarID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RentalContract>()
                .HasOne(rc => rc.Manager)
                .WithMany()
                .HasForeignKey(rc => rc.ManagerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RentalContract>()
                .HasOne(rc => rc.ContractStatus)
                .WithMany()
                .HasForeignKey(rc => rc.ContractStatusID)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь CarInspection  RentalContract
            modelBuilder.Entity<CarInspection>()
                .HasOne(ci => ci.RentalContract) 
                .WithMany()
                .HasForeignKey(ci => ci.ContractID)
                .OnDelete(DeleteBehavior.Cascade);

            // Связи для ContractService 
            modelBuilder.Entity<ContractService>()
                .HasOne(cs => cs.Contract)
                .WithMany()
                .HasForeignKey(cs => cs.ContractID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContractService>()
                .HasOne(cs => cs.Service)
                .WithMany()
                .HasForeignKey(cs => cs.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}