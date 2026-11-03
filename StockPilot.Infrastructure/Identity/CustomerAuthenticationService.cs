using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Customers.DTOs;
using StockPilot.Domain.Entities;
using StockPilot.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Infrastructure.Identity
{
    public class CustomerAuthenticationService(
        UserManager<ApplicationUser> userManager,
        StockPilotDbContext dbContext,
        IJwtTokenService jwtTokenService)
        : ICustomerAuthenticationService
    {
        public async Task<CustomerLoginResponse> LoginAsync(
            CustomerLoginRequest request,
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

            var customer = await dbContext.Customers
                .FirstOrDefaultAsync(
                    c => c.ApplicationUserId == user.Id,
                    cancellationToken) ?? throw new UnauthorizedAccessException(
                    "Customer account not found.");

            // Admin-deactivated accounts cannot self-reactivate.
            if (!customer.IsActive && customer.IsAdminDeactivated)
            {
                throw new UnauthorizedAccessException(
                    "Your account has been deactivated by the administrator. Please contact support.");
            }

            // Automatically reactivate customer-deactivated accounts
            // only within the 45-day reactivation period.
            if (!customer.IsActive)
            {
                if (customer.DeactivatedAtUtc is null)
                {
                    throw new UnauthorizedAccessException(
                        "Your account cannot be reactivated automatically. Please contact support.");
                }

                var reactivationDeadline =
                    customer.DeactivatedAtUtc.Value.AddDays(45);

                if (DateTime.UtcNow > reactivationDeadline)
                {
                    throw new UnauthorizedAccessException(
                        "The 45-day reactivation period has expired. Please contact support.");
                }

                customer.IsActive = true;
                customer.DeactivatedAtUtc = null;
                customer.IsAdminDeactivated = false;

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var token = jwtTokenService.GenerateToken(
                user.Id,
                user.UserName ?? string.Empty,
                user.Email ?? string.Empty,
                customer.Id);

            return new CustomerLoginResponse
            {
                Token = token,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1),
                CustomerId = customer.Id,
                UserName = user.UserName ?? string.Empty,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email
            };
        }
    }
}