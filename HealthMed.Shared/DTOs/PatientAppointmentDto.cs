namespace HealthMed.Shared.DTOs
{
    public class PatientAppointmentDto
    {
        public Guid AppointmentId { get; set; }
        public Guid SlotId { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Specialty { get; set; }
        public decimal ConsultationValor { get; set; }
    }
}
