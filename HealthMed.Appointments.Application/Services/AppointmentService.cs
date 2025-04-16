using Appointments.Infra.Messaging;
using HealthMed.Appointments.Application.Clients;
using HealthMed.Appointments.Domain.Entities;
using HealthMed.Appointments.Domain.Enums;
using HealthMed.Appointments.Domain.Interfaces;
using HealthMed.Schedule.Application.Models;
using HealthMed.Shared.DTOs;

namespace HealthMed.Appointments.Application.Services;

public class AppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly MessagePublisher _publisher;
    private readonly ScheduleClient _scheduleClient;

    public AppointmentService(IAppointmentRepository appointmentRepository, MessagePublisher publisher, ScheduleClient scheduleClient)
    {
        _appointmentRepository = appointmentRepository;
        _publisher = publisher;
        _scheduleClient = scheduleClient;
    }

    public async Task<bool> ScheduleAppointmentAsync(Guid doctorId, Guid patientId, DateTime scheduledTime)
    {
        var availableSlots = await _scheduleClient.GetAvailableSlotsAsync(doctorId);

        if (!availableSlots.Contains(scheduledTime))
            return false;

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            PatientId = patientId,
            ScheduledTime = scheduledTime,
            Status = AppointmentStatus.Pending
        };

        await _appointmentRepository.AddAppointment(appointment);

        var dto = new AppointmentDto
        {
            Id = appointment.Id,
            DoctorId = doctorId,
            PatientId = patientId,
            ScheduledTime = scheduledTime,
            Status = appointment.Status.ToString()
        };

        _publisher.Publish("consulta.agendada", dto);

        return true;
    }

    public async Task<List<Appointment>> GetAppointmentsByDoctorAsync(Guid doctorId)
    {
        return await _appointmentRepository.GetAppointmentsByDoctor(doctorId);
    }

    public async Task<List<Appointment>> GetAppointmentsByPatientAsync(Guid patientId)
    {
        return await _appointmentRepository.GetAppointmentsByPatient(patientId);
    }

    public async Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, AppointmentStatus newStatus, Guid doctorId)
    {
        var appointment = await _appointmentRepository.FindByIdAsync(appointmentId);
        if (appointment == null || appointment.DoctorId != doctorId)
            return false;

        appointment.Status = newStatus;
        await _appointmentRepository.UpdateAppointment(appointment);

        // Publica evento (opcional)
        if (newStatus == AppointmentStatus.Accepted || newStatus == AppointmentStatus.Rejected)
        {
            var dto = new AppointmentDto
            {
                Id = appointment.Id,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                ScheduledTime = appointment.ScheduledTime,
                Status = newStatus.ToString() // se o DTO ainda espera string
            };

            var topic = newStatus == AppointmentStatus.Accepted ? "consulta.aceita" : "consulta.recusada";
            _publisher.Publish(topic, dto);
        }

        return true;
    }

    public async Task<bool> CancelAppointmentAsync(Guid appointmentId, Guid patientId)
    {
        var appointment = await _appointmentRepository.FindByIdAsync(appointmentId);
        if (appointment == null || appointment.PatientId != patientId)
            return false;

        appointment.Status = AppointmentStatus.Cancelled; //  enum ao invés de string
        await _appointmentRepository.UpdateAppointment(appointment);

        var dto = new AppointmentDto
        {
            Id = appointment.Id,
            DoctorId = appointment.DoctorId,
            PatientId = appointment.PatientId,
            ScheduledTime = appointment.ScheduledTime,
            Status = appointment.Status.ToString() //  convertido para string no DTO
        };

        _publisher.Publish("consulta.cancelada", dto);
        return true;
    }

    public async Task<bool> RescheduleAppointmentAsync(Guid appointmentId, DateTime newTime, Guid patientId)
    {
        var appointment = await _appointmentRepository.FindByIdAsync(appointmentId);
        if (appointment == null || appointment.PatientId != patientId)
            return false;

        var isAvailable = await _scheduleClient.IsSlotAvailable(appointment.DoctorId, newTime);
        if (!isAvailable) return false;

        var oldTime = appointment.ScheduledTime;
        appointment.ScheduledTime = newTime;
        appointment.Status = AppointmentStatus.Pending; //  usando o enum

        await _appointmentRepository.UpdateAppointment(appointment);

        var dto = new RescheduleDto
        {
            AppointmentId = appointment.Id,
            DoctorId = appointment.DoctorId,
            OldTime = oldTime,
            NewTime = newTime
        };

        _publisher.Publish("consulta.remarcada", dto);
        return true;
    }

    public async Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(Guid doctorId)
    {
        var allSlots = await _scheduleClient.GetAvailableSlotsAsync(doctorId); // List<DateTime>
        var appointments = await _appointmentRepository.GetAppointmentsByDoctor(doctorId);

        var takenTimes = appointments.Select(a => a.ScheduledTime);

        var available = allSlots
            .Where(slot => !takenTimes.Contains(slot))
            .Select(slot => new AvailableSlotDto
            {
                Id = Guid.NewGuid(), // gerando um ID temporário, ou pode ser omitido se não for usado
                StartTime = slot,
                EndTime = slot.AddMinutes(30) // ou a duração que seu sistema define como padrão
            })
            .ToList();

        return available;
    }
}
