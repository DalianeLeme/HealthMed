using HealthMed.Schedule.Application.Interfaces;
using HealthMed.Schedule.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthMed.Schedule.API.Controllers;

[Authorize(Roles = "Doctor")]
[ApiController]
[Route("api/slots")]
public class AvailableSlotController : ControllerBase
{
    private readonly IAvailableSlotService _service;

    public AvailableSlotController(IAvailableSlotService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (doctorId == null) return Unauthorized();

        var slots = await _service.GetByDoctorAsync(Guid.Parse(doctorId));
        return Ok(slots);
    }

    [Authorize(Roles = "Doctor")]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateAvailableSlotRequest request)
    {
        var doctorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (doctorIdClaim == null) return Unauthorized();

        var slot = new AvailableSlot
        {
            Id = Guid.NewGuid(),
            DoctorId = Guid.Parse(doctorIdClaim),
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        var success = await _service.AddAsync(slot);
        return success ? Ok("Horário cadastrado.") : Conflict("Conflito com outro horário existente.");
    }

    [Authorize(Roles = "Doctor")]
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] UpdateAvailableSlotRequest request)
    {
        var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (doctorId == null) return Unauthorized();

        var slot = await _service.GetByIdAsync(request.Id);
        if (slot == null) return NotFound("Horário não encontrado.");

        if (slot.DoctorId != Guid.Parse(doctorId))
            return Forbid("Você só pode editar seus próprios horários.");

        slot.StartTime = request.StartTime;
        slot.EndTime = request.EndTime;

        var ok = await _service.UpdateAsync(slot);
        if (!ok) return NotFound("Horário não encontrado.");

        return Ok("Horário atualizado.");
    }

    [Authorize(Roles = "Doctor")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (doctorId == null)
            return Unauthorized();

        var slot = await _service.GetByIdAsync(id);
        if (slot == null)
            throw new KeyNotFoundException($"Horário {id} não encontrado.");

        if (slot.DoctorId != Guid.Parse(doctorId))
            return Forbid("Você só pode remover seus próprios horários.");

        await _service.DeleteAsync(id);
        return Ok("Horário removido.");
    }
}