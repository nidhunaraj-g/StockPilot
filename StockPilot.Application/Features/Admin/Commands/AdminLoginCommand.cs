using FluentValidation;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Admin.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Admin.Commands
{
    public record AdminLoginCommand(AdminLoginRequest Request)
        : IRequest<AdminLoginResponse>;

    public class AdminLoginCommandHandler(
        IAdminAuthenticationService authenticationService,
        IValidator<AdminLoginRequest> validator)
        : IRequestHandler<AdminLoginCommand, AdminLoginResponse>
    {
        public async Task<AdminLoginResponse> Handle(
            AdminLoginCommand command,
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
