using HealthMed.Auth.Domain.Entities;
using HealthMed.Auth.Domain.Interfaces;
using HealthMed.Shared.DTOs;

namespace HealthMed.Auth.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> RegisterUserAsync(string name, string email, string password, string role, string? crm = null, string? cpf = null)
    {
        var existingUser = await _userRepository.GetByIdentifierAsync(email);
        if (existingUser != null)
            return false;

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            CRM = role == "Doctor" ? crm : null,
            CPF = role == "Patient" ? cpf : null
        };

        await _userRepository.AddAsync(newUser);
        return true;
    }

    public async Task<User?> AuthenticateUserAsync(string identifier, string password)
    {
        // Buscar por CRM (Médico)
        var user = await _userRepository.FindByCRMAsync(identifier);
        if (user == null)
        {
            // Buscar por CPF (Paciente)
            user = await _userRepository.FindByCPFAsync(identifier);
        }

        if (user == null)
        {
            // Buscar por Email (Paciente)
            user = await _userRepository.FindByEmailAsync(identifier);
        }

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return user;
    }

    public async Task<List<UserDto>> GetAllDoctorsAsync()
    {
        var doctors = await _userRepository.GetAllDoctorsAsync();
        return doctors.Select(d => new UserDto
        {
            Id = d.Id,
            Name = d.Name,
            Email = d.Email,
            Role = d.Role,
            CRM = d.CRM
        }).ToList();
    }


}
