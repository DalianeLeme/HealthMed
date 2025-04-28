using HealthMed.Schedule.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
