using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Orders.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Orders.Commands
{
    public record CreateOrderCommand(
    CreateOrderRequest Request
) : IRequest<OrderResponse>;

    public class CreateOrderCommandHandler(
        IGenericRepository<Order> orderRepository,
        IGenericRepository<Product> productRepository,
        IGenericRepository<StockMovement> stockMovementRepository
    ) : IRequestHandler<CreateOrderCommand, OrderResponse>
    {
        public async Task<OrderResponse> Handle(
            CreateOrderCommand command,
            CancellationToken cancellationToken)
        {
            var request = command.Request;

            if (string.IsNullOrWhiteSpace(request.CustomerName))
                throw new ArgumentException("Customer name is required.");

            if (request.CustomerName.Length > 150)
                throw new ArgumentException("Customer name cannot exceed 150 characters.");

            if (string.IsNullOrWhiteSpace(request.CustomerEmail))
                throw new ArgumentException("Customer email is required.");

            if (request.CustomerEmail.Length > 254)
                throw new ArgumentException("Customer email cannot exceed 254 characters.");

            if (request.CustomerPhone?.Length > 20)
                throw new ArgumentException("Customer phone cannot exceed 20 characters.");

            if (request.Items is null || request.Items.Count == 0)
                throw new ArgumentException("An order must contain at least one item.");

            if (request.Items.Any(item => item.ProductId <= 0 || item.Quantity <= 0))
                throw new ArgumentException("Product IDs and quantities must be greater than zero.");

            if (request.Items.GroupBy(item => item.ProductId).Any(group => group.Count() > 1))
                throw new ArgumentException("Each product can appear only once in an order.");

            var orderItems = new List<OrderItemResponse>();
            var productsToUpdate = new List<(Product Product, int Quantity)>();

            decimal subtotal = 0;

            // Validate products and calculate totals using database prices.
            foreach (var requestedItem in request.Items)
            {
                var product = await productRepository.GetByIdAsync(
                    requestedItem.ProductId,
                    cancellationToken);

                if (product is null || !product.IsActive)
                    throw new KeyNotFoundException(
                        $"Active product with ID {requestedItem.ProductId} was not found.");

                if (product.StockQuantity < requestedItem.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for {product.Name}. Available: {product.StockQuantity}.");

                var lineSubtotal = product.SellingPrice * requestedItem.Quantity;

                if (lineSubtotal > 9999999999999999.99m)
                    throw new ArgumentException("The order item total is too large.");

                subtotal += lineSubtotal;

                if (subtotal > 9999999999999999.99m)
                    throw new ArgumentException("The order total is too large.");

                productsToUpdate.Add((product, requestedItem.Quantity));

                orderItems.Add(new OrderItemResponse
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = requestedItem.Quantity,
                    UnitPrice = product.SellingPrice,
                    Subtotal = lineSubtotal
                });
            }

            // This initial version does not apply tax.
            decimal taxAmount = 0;
            decimal totalAmount = subtotal + taxAmount;

            var order = new Order
            {
                OrderNumber = $"ORD-{Guid.NewGuid():N}".ToUpperInvariant()[..20],
                CustomerName = request.CustomerName.Trim(),
                CustomerEmail = request.CustomerEmail.Trim(),
                CustomerPhone = string.IsNullOrWhiteSpace(request.CustomerPhone)
                    ? null
                    : request.CustomerPhone.Trim(),
                OrderDateUtc = DateTime.UtcNow,
                Status = "Confirmed",
                Subtotal = subtotal,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                Items = []
            };

            foreach (var item in request.Items)
            {
                var product = productsToUpdate
                    .First(p => p.Product.Id == item.ProductId).Product;

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.SellingPrice,
                    Subtotal = product.SellingPrice * item.Quantity
                });
            }

            // Deduct stock and record the corresponding stock movements.
            foreach (var (product, quantity) in productsToUpdate)
            {
                product.StockQuantity -= quantity;
                productRepository.Update(product);

                await stockMovementRepository.AddAsync(
                    new StockMovement
                    {
                        ProductId = product.Id,
                        Quantity = quantity,
                        MovementType = "OUT",
                        Reason = $"Order fulfillment: {order.OrderNumber}"
                    },
                    cancellationToken);
            }

            await orderRepository.AddAsync(order, cancellationToken);

            // All repositories must share the same scoped DbContext.
            // A single SaveChanges persists these changes together.
            await orderRepository.SaveChangesAsync(cancellationToken);

            return new OrderResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,
                OrderDateUtc = order.OrderDateUtc,
                Status = order.Status,
                Subtotal = order.Subtotal,
                TaxAmount = order.TaxAmount,
                TotalAmount = order.TotalAmount,
                Items = orderItems
            };
        }
    }
}
