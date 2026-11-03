using FluentValidation;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Customers.DTOs;

namespace StockPilot.Application.Features.Customers.Commands
{
    public record CustomerLoginCommand(
        CustomerLoginRequest Request) : IRequest<CustomerLoginResponse>;

    public class CustomerLoginCommandHandler(
        ICustomerAuthenticationService authenticationService,
        IValidator<CustomerLoginRequest> validator)
        : IRequestHandler<CustomerLoginCommand, CustomerLoginResponse>
    {
        public async Task<CustomerLoginResponse> Handle(
            CustomerLoginCommand command,
            CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(
                command.Request,
                cancellationToken);

            return await authenticationService.LoginAsync(
                command.Request,
                cancellationToken);
        }
    }
}