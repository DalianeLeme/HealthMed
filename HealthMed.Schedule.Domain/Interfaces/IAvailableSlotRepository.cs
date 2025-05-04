using HealthMed.Schedule.Domain.Entities;

namespace HealthMed.Schedule.Domain.Interfaces;

public interface IAvailableSlotRepository
{
    Task<List<AvailableSlot>> GetByDoctorAsync(Guid doctorId);
    Task AddAsync(AvailableSlot slot);
    Task UpdateAsync(AvailableSlot slot);
    Task DeleteAsync(AvailableSlot slot);
    Task<AvailableSlot?> GetByTimeAsync(Guid doctorId, DateTime startTime);
    Task<AvailableSlot?> GetByIdAsync(Guid id);
    Task<bool> ExistsByIdAsync(Guid slotId);

}