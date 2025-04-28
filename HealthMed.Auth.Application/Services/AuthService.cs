using HealthMed.Auth.Domain.Entities;
using HealthMed.Auth.Domain.Interfaces;
using HealthMed.Shared.DTOs;
using BCrypt.Net;

namespace HealthMed.Auth.Application.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> RegisterUserAsync(
            string name,
            string email,
            string password,
            string role,
            string? crm = null,
            string? specialty = null,
            decimal? consultationFee = null,
            string? cpf = null)
        {
            // não mexemos nessa parte:
            var existingUser = await _userRepository.GetByIdentifierAsync(email);
            if (existingUser != null)
                return false;

            // cria o usuário base
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                CPF = role == "Patient" ? cpf : null
            };

            // só para médicos: injeta o Profile
            if (role == "Doctor")
            {
                if (crm == null ||
                    specialty == null ||
                    consultationFee == null)
                {
                    throw new ArgumentException("CRM, Specialty e ConsultationFee são obrigatórios para médicos.");
                }

                newUser.Profile = new DoctorProfile
                {
                    UserId = newUser.Id,
                    CRM = crm,
                    Specialty = specialty,
                    ConsultationValor = consultationFee.Value
                };
            }

            await _userRepository.AddAsync(newUser);
            return true;
        }

        public async Task<User?> AuthenticateUserAsync(string identifier, string password)
        {
            // mesmíssimo fluxo de busca
            var user = await _userRepository.FindByCRMAsync(identifier)
                    ?? await _userRepository.FindByCPFAsync(identifier)
                    ?? await _userRepository.FindByEmailAsync(identifier);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<List<UserDto>> GetAllDoctorsAsync(string? specialty = null)
        {
            // busca todos os usuários com Role = "Doctor", já incluindo o profile
            var doctors = await _userRepository.GetAllDoctorsAsync();

            // se vier specialty, filtra pela specialty do profile
            if (!string.IsNullOrWhiteSpace(specialty))
            {
                doctors = doctors
                    .Where(d =>
                        d.Profile != null
                        && d.Profile.Specialty.Equals(specialty, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();
            }

            // mapeia para UserDto, incluindo CRM, Specialty e ConsultationValor
            return doctors.Select(d => new UserDto
            {
                Id = d.Id,
                Name = d.Name,
                Email = d.Email,
                Role = d.Role,
                CRM = d.Profile?.CRM,
                Specialty = d.Profile?.Specialty,
                ConsultationValor = d.Profile?.ConsultationValor
            }).ToList();
        }
    }
}
