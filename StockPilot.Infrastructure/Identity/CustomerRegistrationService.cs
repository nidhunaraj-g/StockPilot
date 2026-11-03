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
    public class CustomerRegistrationService(
        UserManager<ApplicationUser> userManager,
        StockPilotDbContext dbContext)
        : ICustomerRegistrationService
    {
        public async Task<CustomerDto> RegisterAsync(
            CustomerRegisterRequest request,
            CancellationToken cancellationToken = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var username = request.UserName.Trim();

            var existingCustomer = await dbContext.Customers
                .AnyAsync(c => c.Email == email, cancellationToken);

            if (existingCustomer)
            {
                throw new InvalidOperationException(
                    "A customer with this email already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                PhoneNumber = request.Phone.Trim()
            };

            var identityResult = await userManager.CreateAsync(
                user,
                request.Password);

            if (!identityResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    identityResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(errors);
            }

            try
            {
                var customer = new Customer
                {
                    ApplicationUserId = user.Id,
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = email,
                    Phone = request.Phone.Trim(),
                    Address = request.Address.Trim(),
                    City = request.City.Trim(),
                    State = request.State.Trim(),
                    Country = request.Country.Trim(),
                    PostalCode = request.PostalCode.Trim(),
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };

                dbContext.Customers.Add(customer);

                await dbContext.SaveChangesAsync(cancellationToken);

                return new CustomerDto
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Address = customer.Address,
                    City = customer.City,
                    State = customer.State,
                    Country = customer.Country,
                    PostalCode = customer.PostalCode,
                    IsActive = customer.IsActive,
                    CreatedAtUtc = customer.CreatedAtUtc,
                    UpdatedAtUtc = customer.UpdatedAtUtc
                };
            }
            catch
            {
                await userManager.DeleteAsync(user);
                throw;
            }
        }
    }
}
