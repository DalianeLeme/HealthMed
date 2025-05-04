using FluentValidation;

public class CreateAvailableSlotRequestValidator
    : AbstractValidator<CreateAvailableSlotRequest>
{
    public CreateAvailableSlotRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.StartTime < x.EndTime)
            .WithMessage("StartTime deve ser anterior a EndTime.");     

        RuleFor(x => x.StartTime)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("StartTime deve ser uma data futura.");
    }
}