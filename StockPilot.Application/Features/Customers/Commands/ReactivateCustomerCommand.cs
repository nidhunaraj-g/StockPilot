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
    public record ReactivateCustomerCommand(int Id) : IRequest<bool>;

    public class ReactivateCustomerCommandHandler(
        IGenericRepository<Customer> repository)
        : IRequestHandler<ReactivateCustomerCommand, bool>
    {
        public async Task<bool> Handle(
            ReactivateCustomerCommand command,
            CancellationToken cancellationToken)
        {
            var customer = await repository.GetByIdAsync(
                command.Id,
                cancellationToken);

            if (customer is null || customer.IsActive)
                return false;

            customer.IsActive = true;
            customer.DeactivatedAtUtc = null;
            customer.IsAdminDeactivated = false;

            repository.Update(customer);

            await repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
