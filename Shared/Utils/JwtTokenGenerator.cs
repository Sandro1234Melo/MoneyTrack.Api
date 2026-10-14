using Microsoft.IdentityModel.Tokens;
using MoneyTrack.Api.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MoneyTrack.Api.Shared.Utils
{
    public static class JwtTokenGenerator
    {
        public static string GenerateToken(User user, string secretKey, string issuer, string audience)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // A validação configurada no Program usa UTF-8. A assinatura precisa
            // usar a mesma codificação para que chaves com caracteres não ASCII
            // (por exemplo, geradas pelo provedor de deploy) não produzam tokens
            // que falham logo na primeira requisição autenticada.
            var key = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(6),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
