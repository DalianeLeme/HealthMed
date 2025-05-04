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
            var profile = await _context.DoctorProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.CRM == crm);

            return profile?.User;
        }

        public async Task<User?> FindByCPFAsync(string cpf)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.CPF == cpf);
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdentifierAsync(string identifier)
        {
            return await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u =>
                       u.Email == identifier
                    || u.CPF == identifier
                    || (u.Profile != null && u.Profile.CRM == identifier)
                );
        }

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<User>> GetAllDoctorsAsync()
        {
            return await _context.Users
                .Include(u => u.Profile)
                .Where(u => u.Role == "Doctor")
                .ToListAsync();
        }
    }
}