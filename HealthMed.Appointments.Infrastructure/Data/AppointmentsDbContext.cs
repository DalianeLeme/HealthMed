using HealthMed.Appointments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthMed.Appointments.Infrastructure.Data
{
    public class AppointmentsDbContext : DbContext
    {
        public AppointmentsDbContext(DbContextOptions<AppointmentsDbContext> options)
            : base(options) { }

        public DbSet<Appointment> Appointments { get; set; }

        // renomeei de AvailableSlots para AvailableSlotProjections
        public DbSet<AvailableSlotProjection> AvailableSlotProjections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.DoctorId, a.ScheduledTime })
                .IsUnique();

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasConversion<string>();

            modelBuilder.Entity<AvailableSlotProjection>()
                .ToTable("AvailableSlotProjections");

            modelBuilder.Entity<Appointment>()
                .Property(a => a.CancellationReason)
                .HasMaxLength(500)
                .IsRequired(false);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.DoctorId, a.ScheduledTime })
                .IsUnique()
                .HasFilter("[Status] IN ('Pending','Accepted')");

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasConversion<string>();


            base.OnModelCreating(modelBuilder);
        }
    }
}
