using HealthMed.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthMed.Auth.Infrastructure.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<DoctorProfile> DoctorProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<User>(b =>
            {
                b.HasKey(u => u.Id);

                b.HasIndex(u => u.Email)
                 .IsUnique();

                b.Property(u => u.Name)
                 .IsRequired()
                 .HasMaxLength(100);

                b.Property(u => u.Email)
                 .IsRequired()
                 .HasMaxLength(200);

                b.HasOne(u => u.Profile)
                 .WithOne(p => p.User)
                 .HasForeignKey<DoctorProfile>(p => p.UserId);
            });

            mb.Entity<DoctorProfile>(b =>
            {
                b.ToTable("DoctorProfiles");

                b.HasKey(p => p.UserId);

                b.Property(p => p.CRM)
                 .IsRequired()
                 .HasMaxLength(20);

                b.Property(p => p.Specialty)
                 .IsRequired()
                 .HasMaxLength(100);

                b.Property(p => p.ConsultationValor)
                 .IsRequired()
                 .HasColumnType("decimal(10,2)");
            });

            base.OnModelCreating(mb);
        }
    }
}