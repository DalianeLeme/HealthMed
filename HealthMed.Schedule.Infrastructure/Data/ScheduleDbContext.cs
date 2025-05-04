using HealthMed.Schedule.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthMed.Schedule.Infrastructure.Data;

public class ScheduleDbContext : DbContext
{
    public ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : base(options) { }

    public DbSet<AvailableSlot> AvailableSlots { get; set; }
}