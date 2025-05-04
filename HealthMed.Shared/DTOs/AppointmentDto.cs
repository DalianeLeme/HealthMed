namespace HealthMed.Shared.DTOs;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public string Status { get; set; } = "Pending";
}