namespace HealthMed.Shared.DTOs
{
    public class AvailableSlotDto
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
