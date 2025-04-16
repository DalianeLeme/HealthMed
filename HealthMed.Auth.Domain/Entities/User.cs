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
        public string? CRM { get; set; } // apenas para médicos
    }
}
