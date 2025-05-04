using FluentValidation;

public class UpdateAvailableSlotRequestValidator
    : AbstractValidator<UpdateAvailableSlotRequest>
{
    public UpdateAvailableSlotRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O Id do slot é obrigatório.");

        RuleFor(x => x)
            .Must(x => x.StartTime < x.EndTime)
            .WithMessage("StartTime deve ser anterior a EndTime.");

        RuleFor(x => x.StartTime)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("StartTime deve ser uma data futura.");
    }
}