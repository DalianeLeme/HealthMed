// HealthMed.Shared.DTOs/UserDto.cs
namespace HealthMed.Shared.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Patient";
        public string? CRM { get; set; }
        public string? Specialty { get; set; }
        public decimal? ConsultationValor { get; set; }
    }
}
