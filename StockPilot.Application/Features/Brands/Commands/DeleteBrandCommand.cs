using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Brands.Commands
{
    public record DeleteBrandCommand(int Id) : IRequest<bool>;
    public class DeleteBrandCommandHandler(IGenericRepository<Brand> repository): IRequestHandler<DeleteBrandCommand, bool>
    {
        public async Task<bool> Handle(DeleteBrandCommand command,CancellationToken cancellationToken)
        {
            var brand = await repository.GetByIdAsync(command.Id,cancellationToken);

            if (brand is null)
                return false;

            if (!brand.IsActive)
                return true;

            brand.IsActive = false;

            repository.Update(brand);

            await repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
