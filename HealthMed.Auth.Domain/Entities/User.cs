namespace HealthMed.Auth.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CPF { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Patient";

        public DoctorProfile? Profile { get; set; }
    }

    public class DoctorProfile
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public string CRM { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public decimal ConsultationValor { get; set; }
    }
}