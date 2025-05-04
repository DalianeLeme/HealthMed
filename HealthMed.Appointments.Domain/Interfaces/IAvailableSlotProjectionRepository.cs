using HealthMed.Appointments.Domain.Entities;

namespace HealthMed.Appointments.Domain.Interfaces
{
    public interface IAvailableSlotProjectionRepository
    {
        Task AddAsync(AvailableSlotProjection slot);
        Task DeleteAsync(Guid slotId);
        Task<AvailableSlotProjection?> GetByIdAsync(Guid id);
        Task<List<AvailableSlotProjection>> GetByDoctorAsync(Guid doctorId);
    }
}