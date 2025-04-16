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

        public async Task<List<UserDto>> GetAllDoctorsAsync()
        {
            var response = await _httpClient.GetAsync("api/auth/doctors");
            response.EnsureSuccessStatusCode();

            var doctors = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            return doctors ?? new List<UserDto>();
        }
    }
}
