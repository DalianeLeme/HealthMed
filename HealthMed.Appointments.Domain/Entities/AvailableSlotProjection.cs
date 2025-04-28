namespace HealthMed.Appointments.Domain.Entities
{
    public class AvailableSlotProjection
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }


        public AvailableSlotProjection(Guid id, Guid doctorId, DateTime start, DateTime end)
        {
            Id = id;
            DoctorId = doctorId;
            StartTime = start;
            EndTime = end;
        }

        public AvailableSlotProjection() { }
    }
}
