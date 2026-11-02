using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Suppliers.Commands
{
    public record DeleteSupplierCommand(int Id) : IRequest<bool>;

    public class DeleteSupplierCommandHandler(IGenericRepository<Supplier> repository) 
        : IRequestHandler<DeleteSupplierCommand, bool>
    {
        public async Task<bool> Handle(DeleteSupplierCommand command,CancellationToken cancellationToken)
        {
            var supplier = await repository.GetByIdAsync(command.Id,cancellationToken);

            if (supplier is null || !supplier.IsActive)
            {
                return false;
            }

            // Soft delete
            supplier.IsActive = false;
            supplier.UpdatedAtUtc = DateTime.UtcNow;

            repository.Update(supplier);

            await repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}