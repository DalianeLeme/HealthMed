using HealthMed.Appointments.Domain.Entities;
using HealthMed.Auth.Domain;
using Microsoft.EntityFrameworkCore;

namespace HealthMed.Appointments.Infrastructure.Data
{
    public class AppointmentsDbContext : DbContext
    {
        public AppointmentsDbContext(DbContextOptions<AppointmentsDbContext> options) : base(options) { }

        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.DoctorId, a.ScheduledTime })
                .IsUnique(); // Um médico não pode ter duas consultas no mesmo horário

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasConversion<string>(); // Mapeia enum como string no banco

            base.OnModelCreating(modelBuilder);
        }
    }
}
