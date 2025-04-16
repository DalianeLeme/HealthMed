namespace HealthMed.Schedule.Application.Models
{
    public class GetAvailableSlotsRequest
    {
        public Guid DoctorId { get; set; }
    }
    public class AvailableSlotDto
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
