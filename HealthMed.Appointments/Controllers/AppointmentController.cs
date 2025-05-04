using HealthMed.Appointments.Application.Clients;
using HealthMed.Appointments.Application.Interfaces;
using HealthMed.Appointments.Application.Models;
using HealthMed.Appointments.Domain.Enums;
using HealthMed.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthMed.Appointments.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("doctorConsultations")]
        public async Task<IActionResult> GetDoctorConsultations()
        {
            var doctorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(doctorIdClaim, out var doctorId))
                return Unauthorized();

            var appointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId);

            var result = appointments
                .Select(a => new DoctorAppointmentDto
                {
                    AppointmentId = a.Id,
                    SlotId = a.SlotId,
                    PatientId = a.PatientId,
                    ScheduledTime = a.ScheduledTime,
                    EndTime = a.EndTime,
                    Status = a.Status.ToString()
                })
                .ToList();

            return Ok(result);
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("patientConsultations")]
        public async Task<IActionResult> GetByPatient([FromServices] AuthClient authClient)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var appointments = await _appointmentService
                .GetAppointmentsByPatientAsync(Guid.Parse(userId));

            var doctors = await authClient.GetAllDoctorsAsync();
            var doctorMap = doctors.ToDictionary(d => d.Id);

            var result = appointments.Select(a =>
            {
                var doc = doctorMap[a.DoctorId];

                return new PatientAppointmentDto
                {
                    AppointmentId = a.Id,
                    SlotId = a.SlotId,
                    DoctorId = a.DoctorId,
                    ScheduledTime = a.ScheduledTime,
                    EndTime = a.EndTime,
                    Status = a.Status.ToString(),
                    Specialty = doc.Specialty,
                    ConsultationValor = doc.ConsultationValor ?? 0m
                };
            }).ToList();

            return Ok(result);
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors(
                [FromServices] AuthClient authClient,
                [FromQuery] string? specialty)
        {
            var doctors = await authClient.GetAllDoctorsAsync(specialty);
            return Ok(doctors);
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("doctor/{doctorId}/available")]
        public async Task<IActionResult> GetAvailableSlots(Guid doctorId)
        {
            var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId);
            var result = slots
                .Select(s => new AvailableSlotDto
                {
                    Id = s.Id,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                })
                .ToList();

            return Ok(result);
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ScheduleRequest request)
        {
            var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(patientId, out var pid))
                return Unauthorized();

            var ok = await _appointmentService.ScheduleAppointmentAsync(request.SlotId, pid);
            return ok
                ? Ok("Consulta agendada.")
                : Conflict("Horário indisponível ou já agendado.");
        }

        [Authorize(Roles = "Doctor")]
        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromQuery] StatusUpdateAction action)
        {
            var doctorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(doctorIdClaim, out var doctorId))
                return Unauthorized();

            var newStatus = action == StatusUpdateAction.Accepted
                ? AppointmentStatus.Accepted
                : AppointmentStatus.Rejected;

            var result = await _appointmentService
                .UpdateAppointmentStatusAsync(id, newStatus, doctorId);

            return result switch
            {
                UpdateStatusResult.NotFound => NotFound("Consulta não encontrada."),
                UpdateStatusResult.Forbidden => Forbid("Você só pode alterar suas próprias consultas."),
                UpdateStatusResult.Success => Ok("Status atualizado com sucesso."),
                _ => StatusCode(500, "Erro inesperado.")
            };
        }

        [Authorize(Roles = "Patient")]
        [HttpPut("reschedule")]
        public async Task<IActionResult> Reschedule([FromBody] RescheduleRequest request)
        {
            var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (patientId == null) return Unauthorized();

            var success = await _appointmentService.RescheduleAppointmentAsync(
                request.AppointmentId, request.NewSlotId, Guid.Parse(patientId)
            );

            return success ? Ok("Consulta remarcada.") : Conflict("Horário não disponível ou consulta não encontrada.");
        }

        [Authorize(Roles = "Patient")]
        [HttpDelete("cancel/{id}")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var patientId))
                return Unauthorized();

            var success = await _appointmentService.CancelAppointmentAsync(id, patientId, request.Justification.Trim());
            return success
                ? Ok("Consulta cancelada.")
                : Conflict("Não foi possível cancelar: consulta não encontrada, já cancelada ou recusada.");
        }
    }
}
