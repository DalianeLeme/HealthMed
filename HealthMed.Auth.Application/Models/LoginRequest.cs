namespace HealthMed.Auth.Application.Models
{
    public class LoginRequest
    {
        public string Identifier { get; set; } = string.Empty; // pode ser Email, CPF ou CRM
        public string Password { get; set; } = string.Empty;
    }

}
