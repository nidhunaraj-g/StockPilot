using MediatR;
using Microsoft.EntityFrameworkCore;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Orders.Commands
{
    public record CancelOrderCommand(int OrderId) : IRequest<bool>;

    public class CancelOrderCommandHandler(
        IGenericRepository<Order> orderRepository,
        IGenericRepository<Product> productRepository,
        IGenericRepository<StockMovement> stockMovementRepository
    ) : IRequestHandler<CancelOrderCommand, bool>
    {
        public async Task<bool> Handle(
            CancelOrderCommand command,
            CancellationToken cancellationToken)
        {
            if (command.OrderId <= 0)
                throw new ArgumentException("Order ID must be greater than zero.");

            var order = await orderRepository
                .GetQueryable()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(
                    o => o.Id == command.OrderId,
                    cancellationToken) ?? throw new KeyNotFoundException(
                    $"Order with ID {command.OrderId} was not found.");

            if (order.Status == "Cancelled")
                throw new InvalidOperationException(
                    "This order has already been cancelled.");

            if (order.Status != "Confirmed")
                throw new InvalidOperationException(
                    $"An order with status '{order.Status}' cannot be cancelled.");

            foreach (var item in order.Items)
            {
                var product = await productRepository.GetByIdAsync(
                    item.ProductId,
                    cancellationToken);

                if (product is null)
                    throw new InvalidOperationException(
                        $"Product with ID {item.ProductId} was not found. Cancellation cannot proceed.");

                if (product.StockQuantity > int.MaxValue - item.Quantity)
                    throw new InvalidOperationException(
                        $"Restoring stock for product '{product.Name}' would exceed the maximum allowed quantity.");

                product.StockQuantity += item.Quantity;
                productRepository.Update(product);

                await stockMovementRepository.AddAsync(
                    new StockMovement
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        MovementType = "IN",
                        Reason = $"Order cancellation: {order.OrderNumber}"
                    },
                    cancellationToken);
            }

            order.Status = "Cancelled";
            orderRepository.Update(order);

            await orderRepository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
