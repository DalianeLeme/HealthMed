using HealthMed.Shared.DTOs;
using System.Net.Http.Json;

namespace HealthMed.Appointments.Application.Clients
{
    public class AuthClient
    {
        private readonly HttpClient _httpClient;

        public AuthClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<UserDto>> GetAllDoctorsAsync(string? specialty = null)
        {
            var url = "api/auth/doctors";
                if (!string.IsNullOrWhiteSpace(specialty))
                url += $"?specialty={Uri.EscapeDataString(specialty)}";
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var doctors = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            return doctors ?? new List<UserDto>();
        }
    }
}