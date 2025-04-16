using HealthMed.Schedule.Domain.Entities;
using HealthMed.Schedule.Domain.Interfaces;

namespace HealthMed.Schedule.Application.Services;

public class AvailableSlotService
{
    private readonly IAvailableSlotRepository _repository;

    public AvailableSlotService(IAvailableSlotRepository repository)
    {
        _repository = repository;
    }

    public Task<List<AvailableSlot>> GetByDoctorAsync(Guid doctorId)
        => _repository.GetByDoctorAsync(doctorId);

    public async Task<bool> AddAsync(AvailableSlot slot)
    {
        var existingSlots = await _repository.GetByDoctorAsync(slot.DoctorId);

        var hasConflict = existingSlots.Any(s =>
            s.StartTime < slot.EndTime && slot.StartTime < s.EndTime
        );

        if (hasConflict) return false;

        await _repository.AddAsync(slot);
        return true;
    }

    public Task UpdateAsync(AvailableSlot slot)
        => _repository.UpdateAsync(slot);

    public Task DeleteAsync(Guid id)
        => _repository.DeleteAsync(id);

    public async Task<bool> RemoveSlotByTimeAsync(Guid doctorId, DateTime startTime)
    {
        var slot = await _repository.GetByTimeAsync(doctorId, startTime);
        if (slot == null) return false;

        await _repository.DeleteAsync(slot.Id);
        return true;
    }

    public Task<AvailableSlot?> GetByIdAsync(Guid id)
    => _repository.GetByIdAsync(id);

}
