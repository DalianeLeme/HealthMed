using HealthMed.Appointments.Domain.Entities;
using HealthMed.Appointments.Domain.Enums;

namespace HealthMed.Appointments.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<bool> ScheduleAppointmentAsync(Guid slotId, Guid patientId); Task<List<Appointment>> GetAppointmentsByDoctorAsync(Guid doctorId);
        Task<List<Appointment>> GetAppointmentsByPatientAsync(Guid patientId);
        // IAppointmentService
        Task<UpdateStatusResult> UpdateAppointmentStatusAsync(
            Guid appointmentId,
            AppointmentStatus newStatus,
            Guid doctorId
        );
        Task<bool> CancelAppointmentAsync(Guid appointmentId, Guid patientId, string justification); 
        Task<bool> RescheduleAppointmentAsync(Guid appointmentId, Guid newSlotId, Guid patientId);
        Task<List<AvailableSlotProjection>> GetAvailableSlotsAsync(Guid doctorId);
    }
}
