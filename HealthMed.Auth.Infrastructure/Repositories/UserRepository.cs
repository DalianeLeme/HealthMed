using HealthMed.Auth.Domain.Entities;
using HealthMed.Auth.Domain.Interfaces;
using HealthMed.Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthMed.Auth.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<User?> FindByCRMAsync(string crm)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.CRM == crm);
        }

        public async Task<User?> FindByCPFAsync(string cpf)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.CPF == cpf);
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdentifierAsync(string identifier)
        {
            return await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == identifier || u.CPF == identifier || u.CRM == identifier
            );
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<User>> GetAllDoctorsAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "Doctor")
                .ToListAsync();
        }

    }
}
