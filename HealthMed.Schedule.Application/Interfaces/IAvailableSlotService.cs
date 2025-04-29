using HealthMed.Schedule.Domain.Entities;

namespace HealthMed.Schedule.Application.Interfaces
{
    public interface IAvailableSlotService
    {
        Task<List<AvailableSlot>> GetByDoctorAsync(Guid doctorId);
        Task<bool> AddAsync(AvailableSlot slot);
        Task DeleteAsync(Guid id);
        Task<bool> RemoveSlotByTimeAsync(Guid doctorId, DateTime startTime);
        Task<bool> UpdateAsync(AvailableSlot slot);
        Task<AvailableSlot?> GetByIdAsync(Guid id);
    }
}
