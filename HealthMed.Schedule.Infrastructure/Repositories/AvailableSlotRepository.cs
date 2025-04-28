using HealthMed.Schedule.Domain.Entities;
using HealthMed.Schedule.Domain.Interfaces;
using HealthMed.Schedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthMed.Schedule.Infrastructure.Repositories;

public class AvailableSlotRepository : IAvailableSlotRepository
{
    private readonly ScheduleDbContext _context;

    public AvailableSlotRepository(ScheduleDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvailableSlot>> GetByDoctorAsync(Guid doctorId)
    {
        return await _context.AvailableSlots
            .Where(s => s.DoctorId == doctorId)
            .ToListAsync();
    }

    public async Task AddAsync(AvailableSlot slot)
    {
        await _context.AvailableSlots.AddAsync(slot);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AvailableSlot slot)
    {
        _context.AvailableSlots.Update(slot);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(AvailableSlot slot)
    {
        _context.AvailableSlots.Remove(slot);
        await _context.SaveChangesAsync();
    }

    public async Task<AvailableSlot?> GetByTimeAsync(Guid doctorId, DateTime startTime)
    {
        return await _context.AvailableSlots
            .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.StartTime == startTime);
    }

    public Task<AvailableSlot?> GetByIdAsync(Guid id)
    {
        return _context.AvailableSlots.FindAsync(id).AsTask();
    }
    public async Task<bool> ExistsByIdAsync(Guid slotId)
       => await _context.AvailableSlots.AnyAsync(s => s.Id == slotId);

}
