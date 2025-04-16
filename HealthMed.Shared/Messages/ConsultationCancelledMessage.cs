namespace HealthMed.Shared.Messages;

public class ConsultationCancelledMessage
{
    public Guid AppointmentId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime ScheduledTime { get; set; }
}
