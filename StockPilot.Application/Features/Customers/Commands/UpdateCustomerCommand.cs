using AutoMapper;
using FluentValidation;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Customers.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Customers.Commands
{
    public record UpdateCustomerCommand(
        int Id,
        UpdateCustomerRequest Request)
        : IRequest<CustomerDto?>;

    public class UpdateCustomerCommandHandler(
        IGenericRepository<Customer> repository,
        IMapper mapper,
        IValidator<UpdateCustomerRequest> validator)
        : IRequestHandler<UpdateCustomerCommand, CustomerDto?>
    {
        public async Task<CustomerDto?> Handle(
            UpdateCustomerCommand command,
            CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(
                command.Request,
                cancellationToken);

            var customer = await repository.GetByIdAsync(
                command.Id,
                cancellationToken);

            if (customer is null || !customer.IsActive)
                return null;

            var email = command.Request.Email.Trim().ToLowerInvariant();

            var existingCustomer = await repository.FindFirstOrDefaultAsync(
                c => c.Email == email && c.Id != command.Id,
                cancellationToken);

            if (existingCustomer is not null)
            {
                throw new ValidationException(
                    "A customer with this email already exists.");
            }

            mapper.Map(command.Request, customer);

            customer.FirstName = command.Request.FirstName.Trim();
            customer.LastName = command.Request.LastName.Trim();
            customer.Email = email;
            customer.Phone = command.Request.Phone.Trim();
            customer.Address = command.Request.Address.Trim();
            customer.City = command.Request.City.Trim();
            customer.State = command.Request.State.Trim();
            customer.Country = command.Request.Country.Trim();
            customer.PostalCode = command.Request.PostalCode.Trim();

            repository.Update(customer);

            await repository.SaveChangesAsync(cancellationToken);

            return mapper.Map<CustomerDto>(customer);
        }
    }
}