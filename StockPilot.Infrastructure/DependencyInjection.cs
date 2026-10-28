using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockPilot.Application.Common.Interfaces;
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
            services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));

            return services;
        }
    }
}
