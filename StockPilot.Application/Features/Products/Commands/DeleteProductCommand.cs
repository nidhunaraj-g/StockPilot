using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Products.Commands
{
    public record DeleteProductCommand(int Id) : IRequest<bool>;

    public class DeleteProductCommandHandler(IGenericRepository<Product> productRepository): IRequestHandler<DeleteProductCommand, bool>
    {
        public async Task<bool> Handle(DeleteProductCommand command,CancellationToken cancellationToken)
        {
            var product = await productRepository.GetByIdAsync(command.Id,cancellationToken);

            if (product is null)
                return false;

            if (!product.IsActive)
                return true;

            product.IsActive = false;

            productRepository.Update(product);

            await productRepository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
