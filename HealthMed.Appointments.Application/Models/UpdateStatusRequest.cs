using HealthMed.Appointments.Domain.Enums;

namespace HealthMed.Appointments.Application.Models
{
    public class UpdateStatusRequest
    {
        public Guid AppointmentId { get; set; }
        public AppointmentStatus NewStatus { get; set; } = AppointmentStatus.Accepted;
    }
}
