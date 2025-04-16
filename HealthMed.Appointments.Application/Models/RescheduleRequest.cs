namespace HealthMed.Appointments.Application.Models;

public class RescheduleRequest
{
    public Guid AppointmentId { get; set; }
    public DateTime NewTime { get; set; }
}
