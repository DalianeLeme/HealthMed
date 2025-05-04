using HealthMed.Auth.Domain.Entities;

namespace HealthMed.Auth.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> FindByCRMAsync(string crm);
        Task<User?> FindByCPFAsync(string cpf);
        Task<User?> FindByEmailAsync(string email);
        Task<User?> GetByIdentifierAsync(string identifier);
        Task AddAsync(User user);
        Task<List<User>> GetAllDoctorsAsync();
    }
}