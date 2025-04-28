using HealthMed.Appointments.Domain.Enums;

namespace HealthMed.Appointments.Domain.Entities
{
    public class Appointment
    {
        public Guid Id { get; set; }
        public Guid SlotId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime EndTime { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public string? CancellationReason { get; set; }
    }
}
