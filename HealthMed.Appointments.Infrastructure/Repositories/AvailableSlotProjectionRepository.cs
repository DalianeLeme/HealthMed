using HealthMed.Appointments.Domain.Entities;
using HealthMed.Appointments.Domain.Interfaces;
using HealthMed.Appointments.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthMed.Appointments.Infrastructure.Repositories
{
    public class AvailableSlotProjectionRepository : IAvailableSlotProjectionRepository
    {
        private readonly AppointmentsDbContext _db;

        public AvailableSlotProjectionRepository(AppointmentsDbContext db)
            => _db = db;

        public async Task AddAsync(AvailableSlotProjection slot)
        {
            _db.AvailableSlotProjections.Add(slot);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid slotId)
        {
            var entity = await _db.AvailableSlotProjections.FindAsync(slotId);
            if (entity != null)
            {
                _db.AvailableSlotProjections.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<AvailableSlotProjection?> GetByIdAsync(Guid id)
            => await _db.AvailableSlotProjections.FindAsync(id);

        public async Task<List<AvailableSlotProjection>> GetByDoctorAsync(Guid doctorId)
            => await _db.AvailableSlotProjections
                        .Where(s => s.DoctorId == doctorId)
                        .ToListAsync();
    }
}