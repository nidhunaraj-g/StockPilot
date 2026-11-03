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
    public record DeleteCustomerCommand(int Id) : IRequest<bool>;

    public class DeleteCustomerCommandHandler(
        IGenericRepository<Customer> repository)
        : IRequestHandler<DeleteCustomerCommand, bool>
    {
        public async Task<bool> Handle(
            DeleteCustomerCommand command,
            CancellationToken cancellationToken)
        {
            var customer = await repository.GetByIdAsync(
                command.Id,
                cancellationToken);

            if (customer is null || !customer.IsActive)
                return false;

            customer.IsActive = false;
            customer.DeactivatedAtUtc = DateTime.UtcNow;
            customer.IsAdminDeactivated = true;

            repository.Update(customer);

            await repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}