namespace HealthMed.Shared.Messages;

public class ConsultationRescheduledMessage
{
    public Guid AppointmentId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime OldTime { get; set; }
    public DateTime NewTime { get; set; }
}
