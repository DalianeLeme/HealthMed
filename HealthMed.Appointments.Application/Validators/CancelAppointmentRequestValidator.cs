using FluentValidation;
using HealthMed.Appointments.Application.Models;

public class CancelAppointmentRequestValidator : AbstractValidator<CancelAppointmentRequest>
{
    public CancelAppointmentRequestValidator()
    {
        RuleFor(x => x.Justification)
            .NotEmpty()
            .WithMessage("A justificativa é obrigatória ao cancelar uma consulta.")
            .MaximumLength(500)
            .WithMessage("A justificativa não pode exceder 500 caracteres.");
    }
}