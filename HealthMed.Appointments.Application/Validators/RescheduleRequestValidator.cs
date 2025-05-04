using FluentValidation;
using HealthMed.Appointments.Application.Models;

public class RescheduleRequestValidator : AbstractValidator<RescheduleRequest>
{
    public RescheduleRequestValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithMessage("O AppointmentId é obrigatório.");

        RuleFor(x => x.NewSlotId)
            .NotEmpty()
            .WithMessage("O NewSlotId é obrigatório para reagendamento.");
    }
}