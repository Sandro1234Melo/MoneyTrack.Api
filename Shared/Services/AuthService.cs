using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Infrastructure.Data;
using MoneyTrack.Api.Shared.Utils;

namespace MoneyTrack.Api.Shared.Services
{
    public class AuthService
    {
        private readonly MoneyTrackDbContext _context;

        public AuthService(MoneyTrackDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            bool valid = AppPasswordHasher.Verify(password, user.PasswordHash);
            return valid ? user : null;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        
        public bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}