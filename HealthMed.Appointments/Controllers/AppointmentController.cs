using HealthMed.Appointments.Application.Models;
using HealthMed.Appointments.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HealthMed.Shared.DTOs;
using HealthMed.Appointments.Domain.Entities;
using HealthMed.Appointments.Domain.Enums;
using HealthMed.Appointments.Application.Clients;

namespace HealthMed.Appointments.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppointmentService _appointmentService;

        public AppointmentController(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ScheduleRequest request)
        {
            var patientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (patientIdClaim == null) return Unauthorized();

            var success = await _appointmentService.ScheduleAppointmentAsync(
                request.DoctorId,
                Guid.Parse(patientIdClaim),
                request.ScheduledTime
            );

            return success ? Ok("Consulta agendada.") : Conflict("Horário indisponível.");
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("doctor/{doctorId}/available")]
        public async Task<IActionResult> GetAvailableSlots(Guid doctorId)
        {
            var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId);
            return Ok(slots);
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("patient")]
        public async Task<IActionResult> GetByPatient()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var appointments = await _appointmentService.GetAppointmentsByPatientAsync(Guid.Parse(userId));
            return Ok(appointments);
        }

        [Authorize(Roles = "Doctor,Patient")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] AppointmentStatus newStatus)
        {
            var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (doctorId == null)
                return Unauthorized();

            var result = await _appointmentService.UpdateAppointmentStatusAsync(
                id, newStatus, Guid.Parse(doctorId)
            );

            return result ? Ok("Status atualizado com sucesso.") : Forbid("Você só pode alterar suas próprias consultas.");
        }


        [Authorize(Roles = "Patient")]
        [HttpDelete("cancel/{id}")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var result = await _appointmentService.CancelAppointmentAsync(id, Guid.Parse(userId));
            return result ? Ok("Consulta cancelada.") : Forbid("Você só pode cancelar suas próprias consultas.");
        }

        [Authorize(Roles = "Patient")]
        [HttpPut("reschedule")]
        public async Task<IActionResult> Reschedule([FromBody] RescheduleRequest request)
        {
            var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (patientId == null) return Unauthorized();

            var success = await _appointmentService.RescheduleAppointmentAsync(
                request.AppointmentId, request.NewTime, Guid.Parse(patientId)
            );

            return success ? Ok("Consulta remarcada.") : Conflict("Horário não disponível ou consulta não encontrada.");
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors([FromServices] AuthClient authClient)
        {
            var doctors = await authClient.GetAllDoctorsAsync();
            return Ok(doctors);
        }
    }
}
