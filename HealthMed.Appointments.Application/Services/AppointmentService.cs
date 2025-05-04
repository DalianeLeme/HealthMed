using HealthMed.Appointments.Application.Interfaces;
using HealthMed.Appointments.Domain.Entities;
using HealthMed.Appointments.Domain.Enums;
using HealthMed.Appointments.Domain.Interfaces;
using HealthMed.Shared.Events;
using HealthMed.Shared.Messaging;
using AppointmentSlot = HealthMed.Appointments.Domain.Entities.AvailableSlotProjection;

namespace HealthMed.Appointments.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAvailableSlotProjectionRepository _slotRepo;
        private readonly IEventPublisher _publisher;
        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IAvailableSlotProjectionRepository slotRepo,
            IEventPublisher publisher)
        {
            _appointmentRepository = appointmentRepository;
            _slotRepo = slotRepo;
            _publisher = publisher;
        }
        public async Task<bool> ScheduleAppointmentAsync(Guid slotId, Guid patientId)
        {
            var slot = await _slotRepo.GetByIdAsync(slotId);
            if (slot is null) return false;

            if (await _appointmentRepository.ExistsByDoctorAndTimeAsync(slot.DoctorId, slot.StartTime))
                return false;

            var appt = new Appointment
            {
                Id = Guid.NewGuid(),
                SlotId = slot.Id,
                DoctorId = slot.DoctorId,
                PatientId = patientId,
                ScheduledTime = slot.StartTime,
                EndTime = slot.EndTime,      // salva aqui
                Status = AppointmentStatus.Pending
            };

            await _appointmentRepository.AddAppointment(appt);

            _publisher.Publish(
                nameof(ConsultationCreated),
                new ConsultationCreated(
                appt.Id,
                appt.SlotId,
                appt.DoctorId,
                appt.ScheduledTime)
            );

            return true;
        }

        public async Task<UpdateStatusResult> UpdateAppointmentStatusAsync(
            Guid appointmentId,
            AppointmentStatus newStatus,
            Guid doctorId)
        {
            var appt = await _appointmentRepository.FindByIdAsync(appointmentId);
            if (appt == null)
                return UpdateStatusResult.NotFound;

            if (appt.DoctorId != doctorId)
                return UpdateStatusResult.Forbidden;

                 if (appt.Status != AppointmentStatus.Pending)
                        return UpdateStatusResult.Forbidden;

            appt.Status = newStatus;
            await _appointmentRepository.UpdateAppointment(appt);

            if (newStatus == AppointmentStatus.Accepted)
            {
                _publisher.Publish(
                    nameof(ConsultationAccepted),
                    new ConsultationAccepted(
                        appt.Id,
                        appt.DoctorId,
                        appt.PatientId,
                        appt.ScheduledTime
                    )
                );
            }
            else if (newStatus == AppointmentStatus.Rejected)
            {
                var slot = await _slotRepo.GetByIdAsync(appt.SlotId);
                var endTime = slot?.EndTime ?? appt.ScheduledTime;

                _publisher.Publish(
                    nameof(ConsultationRejected),
                    new ConsultationRejected(
                        appt.Id,
                        appt.DoctorId,
                        appt.PatientId,
                        appt.ScheduledTime,
                        endTime
                    )
                );
            }

            return UpdateStatusResult.Success;
        }

        public async Task<List<AppointmentSlot>> GetAvailableSlotsAsync(Guid doctorId)
            => await _slotRepo.GetByDoctorAsync(doctorId);

        public async Task<List<Appointment>> GetAppointmentsByDoctorAsync(Guid doctorId)
            => await _appointmentRepository.GetAppointmentsByDoctor(doctorId);

        public async Task<List<Appointment>> GetAppointmentsByPatientAsync(Guid patientId)
            => await _appointmentRepository.GetAppointmentsByPatient(patientId);

        public async Task<bool> CancelAppointmentAsync(Guid appointmentId, Guid patientId, string justification)
        {
            var appt = await _appointmentRepository.FindByIdAsync(appointmentId);
            if (appt is null || appt.PatientId != patientId)
                return false;

            if (appt.Status == AppointmentStatus.Cancelled
             || appt.Status == AppointmentStatus.Rejected)
            {
                return false;
            }

            appt.Status = AppointmentStatus.Cancelled;
            appt.CancellationReason = justification;
            await _appointmentRepository.UpdateAppointment(appt);

            _publisher.Publish(
                nameof(ConsultationCancelled),
                new ConsultationCancelled(
                    appt.Id,
                    appt.DoctorId,
                    appt.PatientId,
                    appt.ScheduledTime,
                    appt.EndTime
                )
            );

            return true;
        }

        public async Task<bool> RescheduleAppointmentAsync(
            Guid appointmentId,
            Guid newSlotId,
            Guid patientId)
        {
            var appt = await _appointmentRepository.FindByIdAsync(appointmentId);
            if (appt is null
             || appt.PatientId != patientId
             || appt.Status == AppointmentStatus.Rejected
             || appt.Status == AppointmentStatus.Cancelled)
            {
                return false;
            }

            var oldStart = appt.ScheduledTime;
            var oldEnd = appt.EndTime;

            var newSlot = await _slotRepo.GetByIdAsync(newSlotId);
            if (newSlot is null)
                return false;

            _publisher.Publish(
                nameof(ConsultationCancelled),
                new ConsultationCancelled(
                    appt.Id,
                    appt.DoctorId,
                    appt.PatientId,
                    oldStart,
                    oldEnd
                )
            );

            _publisher.Publish(
                nameof(ConsultationCreated),
                new ConsultationCreated(
                    appt.Id,
                    newSlot.Id,
                    appt.DoctorId,
                    newSlot.StartTime
                )
            );

            appt.SlotId = newSlot.Id;
            appt.ScheduledTime = newSlot.StartTime;
            appt.EndTime = newSlot.EndTime;
            appt.Status = AppointmentStatus.Pending;
            await _appointmentRepository.UpdateAppointment(appt);

            return true;
        }
    }
}
