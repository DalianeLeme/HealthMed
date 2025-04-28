using HealthMed.Shared.DTOs;
using System.Text.Json.Serialization;

namespace HealthMed.Auth.Application.Models
{
    public class RegisterRequest
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;

        public string? CPF { get; set; }

        // só para médicos:
        public string? CRM { get; set; }
        public string? Specialty { get; set; }
        public decimal? ConsultationValor { get; set; }
    }
}
