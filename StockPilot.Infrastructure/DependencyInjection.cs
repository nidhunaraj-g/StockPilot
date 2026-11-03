using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Infrastructure.Identity;
using StockPilot.Infrastructure.Persistence;
using StockPilot.Infrastructure.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection is missing.");

            services.AddDbContext<StockPilotDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped(
                typeof(IGenericRepository<>),
                typeof(GenericRepository<>));

            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<StockPilotDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

            services.AddScoped<
                ICustomerRegistrationService,
                CustomerRegistrationService>();

            services.AddScoped<
                IAdminAuthenticationService,
                AdminAuthenticationService>();

            services.AddScoped<
                ICustomerAuthenticationService,
                CustomerAuthenticationService>();

            services.AddScoped<
                IJwtTokenService,
                JwtTokenService>();

            services.AddHostedService<AdminBootstrapper>();

            return services;
        }
    }
}