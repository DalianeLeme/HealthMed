namespace HealthMed.Shared.DTOs;

public class RescheduleDto
{
    public Guid AppointmentId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime OldTime { get; set; }
    public DateTime NewTime { get; set; }
}
