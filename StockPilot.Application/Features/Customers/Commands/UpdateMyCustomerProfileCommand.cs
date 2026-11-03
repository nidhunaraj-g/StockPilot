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
    public record UpdateMyCustomerProfileCommand(
        int UserId,
        int CustomerId,
        UpdateMyCustomerRequest Request) : IRequest<CustomerDto?>;

    public class UpdateMyCustomerProfileCommandHandler(
        IGenericRepository<Customer> repository,
        IMapper mapper,
        IValidator<UpdateMyCustomerRequest> validator)
        : IRequestHandler<UpdateMyCustomerProfileCommand, CustomerDto?>
    {
        public async Task<CustomerDto?> Handle(
            UpdateMyCustomerProfileCommand command,
            CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(
                command.Request,
                cancellationToken);

            var customer = await repository.FindFirstOrDefaultAsync(
                c => c.Id == command.CustomerId &&
                     c.ApplicationUserId == command.UserId &&
                     c.IsActive,
                cancellationToken);

            if (customer is null)
                return null;

            customer.FirstName = command.Request.FirstName.Trim();
            customer.LastName = command.Request.LastName.Trim();
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