using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Stocks.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Stocks.Commands
{
    public record AdjustStockCommand(AdjustStockRequest Request): IRequest<StockMovementResponse>;

    public class AdjustStockCommandHandler(IGenericRepository<Product> productRepository,IGenericRepository<StockMovement> movementRepository)
        : IRequestHandler<AdjustStockCommand, StockMovementResponse>
    {
        public async Task<StockMovementResponse> Handle(AdjustStockCommand command,CancellationToken cancellationToken)
        {
            var request = command.Request;

            if (request.ProductId <= 0)
                throw new ArgumentException("Invalid product ID.");

            if (request.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            var movementType = request.MovementType.Trim().ToUpperInvariant();

            if (movementType != "IN" && movementType != "OUT")
                throw new ArgumentException("MovementType must be IN or OUT.");

            var product = await productRepository.GetByIdAsync(
                request.ProductId, cancellationToken);

            if (product is null || !product.IsActive)
                throw new KeyNotFoundException("Active product not found.");

            if (movementType == "OUT" &&
                product.StockQuantity < request.Quantity)
            {
                throw new InvalidOperationException("Insufficient stock available.");
            }

            if (movementType == "IN" &&
                product.StockQuantity > int.MaxValue - request.Quantity)
            {
                throw new InvalidOperationException("Stock quantity exceeds the allowed limit.");
            }

            product.StockQuantity = movementType == "IN"
                ? product.StockQuantity + request.Quantity
                : product.StockQuantity - request.Quantity;

            var movement = new StockMovement
            {
                ProductId = product.Id,
                Quantity = request.Quantity,
                MovementType = movementType,
                Reason = request.Reason?.Trim()
            };

            productRepository.Update(product);
            await movementRepository.AddAsync(movement, cancellationToken);

            // Both repositories share the same scoped DbContext.
            // Saving through one persists both changes together.
            await productRepository.SaveChangesAsync(cancellationToken);

            return new StockMovementResponse
            {
                Id = movement.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = movement.Quantity,
                MovementType = movement.MovementType,
                Reason = movement.Reason,
                CreatedAtUtc = movement.CreatedAtUtc
            };
        }
    }
}
