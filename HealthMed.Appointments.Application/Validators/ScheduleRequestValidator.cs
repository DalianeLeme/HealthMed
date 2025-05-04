using FluentValidation;

public class ScheduleRequestValidator : AbstractValidator<ScheduleRequest>
{
    public ScheduleRequestValidator()
    {
        RuleFor(x => x.SlotId)
            .NotEmpty()
            .WithMessage("O SlotId é obrigatório para agendamento.");
    }
}