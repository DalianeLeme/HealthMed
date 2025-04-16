using HealthMed.Appointments.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace HealthMed.Appointments.Infrastructure
{
    public class AppointmentsDbContextFactory : IDesignTimeDbContextFactory<AppointmentsDbContext>
    {
        public AppointmentsDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "HealthMed.Appointments"))
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppointmentsDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);

            return new AppointmentsDbContext(optionsBuilder.Options);
        }
    }
}
