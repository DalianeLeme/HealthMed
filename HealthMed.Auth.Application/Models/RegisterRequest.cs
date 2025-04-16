namespace HealthMed.Auth.Application.Models
{
    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CPF { get; set; }        
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Patient";
        public string? CRM { get; set; }         // Apenas para médicos
    }

}
