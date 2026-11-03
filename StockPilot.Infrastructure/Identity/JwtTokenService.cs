using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StockPilot.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Infrastructure.Identity
{
    public class JwtTokenService(IConfiguration configuration)
        : IJwtTokenService
    {
        public string GenerateToken(
            int userId,
            string userName,
            string email,
            int? customerId = null,
            IEnumerable<string>? roles = null)
        {
            var jwtSettings = configuration.GetSection("Jwt");

            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException(
                    "JWT SecretKey is missing.");

            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Name, userName),
                new(ClaimTypes.Email, email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (customerId.HasValue)
            {
                claims.Add(new Claim(
                    "CustomerId",
                    customerId.Value.ToString()));
            }

            if (roles is not null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var expiresAtUtc = DateTime.UtcNow.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}