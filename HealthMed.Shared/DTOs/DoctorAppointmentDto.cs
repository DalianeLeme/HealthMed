namespace HealthMed.Shared.DTOs
{
    public class DoctorAppointmentDto
    {
        public Guid AppointmentId { get; set; }
        public Guid SlotId { get; set; }
        public Guid PatientId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }  
    }
}