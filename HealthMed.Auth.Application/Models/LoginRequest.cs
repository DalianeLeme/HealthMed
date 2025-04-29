namespace HealthMed.Auth.Application.Models
{
    public class LoginRequest
    {
        public string Identifier { get; set; } = string.Empty; 
        public string Password { get; set; } = string.Empty;
    }

}
