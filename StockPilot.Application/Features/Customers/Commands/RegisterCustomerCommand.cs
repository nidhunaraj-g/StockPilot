using FluentValidation;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Customers.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Customers.Commands
{
    public record RegisterCustomerCommand(CustomerRegisterRequest Request)
        : IRequest<CustomerDto>;

    public class RegisterCustomerCommandHandler(
        ICustomerRegistrationService registrationService,
        IValidator<CustomerRegisterRequest> validator)
        : IRequestHandler<RegisterCustomerCommand, CustomerDto>
    {
        public async Task<CustomerDto> Handle(
            RegisterCustomerCommand command,
            CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(
                command.Request,
                cancellationToken);

            return await registrationService.RegisterAsync(
                command.Request,
                cancellationToken);
        }
    }
}