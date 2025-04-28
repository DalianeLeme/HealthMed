namespace HealthMed.Schedule.Domain.Entities;

public class AvailableSlot
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public AvailableSlot(Guid id, Guid doctorId, DateTime startTime, DateTime endTime)
    {
        Id = id;
        DoctorId = doctorId;
        StartTime = startTime;
        EndTime = endTime;
    }
    public AvailableSlot() { }
}
