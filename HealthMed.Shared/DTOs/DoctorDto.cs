namespace HealthMed.Shared.DTOs
{
    public class DoctorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CRM { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public decimal ConsultationValor { get; set; }
    }
}