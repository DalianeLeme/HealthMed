﻿namespace HealthMed.Auth.Application.Models
{
    public class RegisterRequest
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;

        public string? CPF { get; set; }


        public string? CRM { get; set; }
        public string? Specialty { get; set; }
        public decimal? ConsultationValor { get; set; }
    }
}