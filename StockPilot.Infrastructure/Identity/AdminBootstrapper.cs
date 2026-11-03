using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Infrastructure.Identity
{
    public class AdminBootstrapper(
       IServiceScopeFactory scopeFactory,
       IConfiguration configuration) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var userName = configuration["AdminBootstrap:UserName"];
            var email = configuration["AdminBootstrap:Email"];
            var password = configuration["AdminBootstrap:Password"];

            if (string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "AdminBootstrap configuration is missing. " +
                    "Configure AdminBootstrap:UserName, Email and Password.");
            }

            using var scope = scopeFactory.CreateScope();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole<int>>>();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            const string adminRole = "Admin";

            // Create the Admin role if it does not exist.
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                var roleResult = await roleManager.CreateAsync(
                    new IdentityRole<int>(adminRole));

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Could not create Admin role: " +
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }

            // Find the configured Admin account.
            var adminUser = await userManager.FindByNameAsync(userName);

            if (adminUser is null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(
                    adminUser,
                    password);

                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Could not create initial Admin account: " +
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }

            // Ensure the account has the Admin role.
            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                var addRoleResult = await userManager.AddToRoleAsync(
                    adminUser,
                    adminRole);

                if (!addRoleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Could not assign Admin role: " +
                        string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}