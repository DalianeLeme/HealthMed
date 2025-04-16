namespace HealthMed.Shared.Messages;

public class ConsultationCreatedMessage
{
    public Guid AppointmentId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime ScheduledTime { get; set; }
}
