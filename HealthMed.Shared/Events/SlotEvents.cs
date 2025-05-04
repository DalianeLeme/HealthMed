namespace HealthMed.Shared.Events
{
    public record SlotCreated(Guid Id, Guid DoctorId, DateTime StartTime, DateTime EndTime);
    public record SlotDeleted(Guid Id, Guid DoctorId);
    public record ConsultationCreated(Guid ConsultationId, Guid SlotId, Guid DoctorId, DateTime ScheduledTime); 
    public record ConsultationAccepted(Guid ConsultationId, Guid DoctorId, Guid PatientId, DateTime ScheduledTime);
    public record ConsultationRejected(Guid ConsultationId, Guid DoctorId, Guid PatientId, DateTime ScheduledTime, DateTime EndTime);
    public record ConsultationCancelled(Guid ConsultationId, Guid DoctorId, Guid PatientId, DateTime ScheduledTime, DateTime EndTime);
    public record ConsultationRescheduled(Guid ConsultationId, Guid DoctorId, DateTime OldTime, DateTime NewTime);
}