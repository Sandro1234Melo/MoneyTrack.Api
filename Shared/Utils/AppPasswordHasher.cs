using System.Security.Cryptography;
using System.Text;

namespace MoneyTrack.Api.Shared.Utils
{
    public static class AppPasswordHasher
    {
        public static string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool Verify(string password, string storedHash)
        {
            var hashOfInput = Hash(password);
            return hashOfInput == storedHash;
        }
    }
}
