using HealthMed.Appointments.Domain.Entities;

namespace HealthMed.Appointments.Domain.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<List<Appointment>> GetAppointmentsByDoctor(Guid doctorId);
        Task<List<Appointment>> GetAppointmentsByPatient(Guid patientId);
        Task AddAppointment(Appointment appointment);
        Task UpdateAppointment(Appointment appointment);
        Task<Appointment?> FindByIdAsync(Guid appointmentId);
        Task<bool> ExistsByDoctorAndTimeAsync(Guid doctorId, DateTime scheduledTime);
    }
}