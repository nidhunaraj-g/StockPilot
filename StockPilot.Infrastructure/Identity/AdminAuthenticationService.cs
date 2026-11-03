using Microsoft.AspNetCore.Identity;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Admin.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Infrastructure.Identity
{
    public class AdminAuthenticationService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService)
        : IAdminAuthenticationService
    {
        public async Task<AdminLoginResponse> LoginAsync(
            AdminLoginRequest request,
            CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByNameAsync(
                request.UserName.Trim()) ?? throw new UnauthorizedAccessException(
                    "Invalid username or password.");
            var passwordValid = await userManager.CheckPasswordAsync(
                user,
                request.Password);

            if (!passwordValid)
            {
                throw new UnauthorizedAccessException(
                    "Invalid username or password.");
            }

            var roles = await userManager.GetRolesAsync(user);

            if (!roles.Contains("Admin"))
            {
                throw new UnauthorizedAccessException(
                    "You do not have Admin access.");
            }

            var expiresAtUtc = DateTime.UtcNow.AddHours(1);

            var token = jwtTokenService.GenerateToken(
                user.Id,
                user.UserName ?? string.Empty,
                user.Email ?? string.Empty,
                roles: roles);

            return new AdminLoginResponse
            {
                Token = token,
                ExpiresAtUtc = expiresAtUtc,
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = [.. roles]
            };
        }
    }
}
