using FluentValidation;
using HealthMed.Auth.Application.Models;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(6).WithMessage("Senha deve ter ao menos 6 caracteres.");

        RuleFor(x => x.CPF)
            .NotEmpty().WithMessage("CPF é obrigatório.");

        
        When(x => !string.IsNullOrEmpty(x.CRM), () =>
        {
            RuleFor(x => x.CRM)
                .NotEmpty().WithMessage("CRM é obrigatório para médicos.");

            RuleFor(x => x.Specialty)
                .NotEmpty().WithMessage("Especialidade é obrigatória para médicos.");

            RuleFor(x => x.ConsultationValor)
                .NotNull().WithMessage("Valor da consulta é obrigatório para médicos.")
                .GreaterThan(0).WithMessage("Valor da consulta deve ser maior que zero.");
        });
    }
}