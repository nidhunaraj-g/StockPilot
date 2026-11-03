using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Customers.Commands
{
    public record DeactivateMyAccountCommand(int UserId) : IRequest<bool>;

    public class DeactivateMyAccountCommandHandler(
        IGenericRepository<Customer> repository)
        : IRequestHandler<DeactivateMyAccountCommand, bool>
    {
        public async Task<bool> Handle(
            DeactivateMyAccountCommand command,
            CancellationToken cancellationToken)
        {
            var customer = await repository.FindFirstOrDefaultAsync(
                c => c.ApplicationUserId == command.UserId,
                cancellationToken);

            if (customer is null || !customer.IsActive)
                return false;

            customer.IsActive = false;
            customer.DeactivatedAtUtc = DateTime.UtcNow;
            customer.IsAdminDeactivated = false;

            repository.Update(customer);

            await repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}