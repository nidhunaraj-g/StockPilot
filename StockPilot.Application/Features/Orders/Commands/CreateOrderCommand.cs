using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Orders.DTOs;
using StockPilot.Domain.Entities;

namespace StockPilot.Application.Features.Orders.Commands
{
    public record CreateOrderCommand(int CustomerId,CreateOrderRequest Request) : IRequest<OrderResponse>;

    public class CreateOrderCommandHandler(
        IGenericRepository<Order> orderRepository,
        IGenericRepository<Customer> customerRepository,
        IGenericRepository<Product> productRepository,
        IGenericRepository<StockMovement> stockMovementRepository
    ) : IRequestHandler<CreateOrderCommand, OrderResponse>
    {
        public async Task<OrderResponse> Handle(CreateOrderCommand command,CancellationToken cancellationToken)
        {
            var request = command.Request;

            if (command.CustomerId <= 0)
                throw new ArgumentException("Invalid customer.");

            var customer = await customerRepository.GetByIdAsync(command.CustomerId,cancellationToken);

            if (customer is null || !customer.IsActive)
                throw new KeyNotFoundException("Active customer was not found.");

            if (string.IsNullOrWhiteSpace(request.DeliveryAddress))
                throw new ArgumentException("Delivery address is required.");

            if (request.DeliveryAddress.Length > 300)
                throw new ArgumentException("Delivery address cannot exceed 300 characters.");

            if (string.IsNullOrWhiteSpace(request.DeliveryCity) ||
                request.DeliveryCity.Length > 100)
                throw new ArgumentException("A valid delivery city is required (maximum 100 characters).");

            if (string.IsNullOrWhiteSpace(request.DeliveryState) ||
                request.DeliveryState.Length > 100)
                throw new ArgumentException("A valid delivery state is required (maximum 100 characters).");

            if (string.IsNullOrWhiteSpace(request.DeliveryCountry) ||
                request.DeliveryCountry.Length > 100)
                throw new ArgumentException("A valid delivery country is required (maximum 100 characters).");

            if (string.IsNullOrWhiteSpace(request.DeliveryPostalCode) ||
                request.DeliveryPostalCode.Length > 20)
                throw new ArgumentException("A valid delivery postal code is required (maximum 20 characters).");

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

            decimal taxAmount = 0;
            decimal totalAmount = subtotal + taxAmount;

            var order = new Order
            {
                CustomerId = customer.Id,

                OrderNumber = $"ORD-{Guid.NewGuid():N}".ToUpperInvariant()[..20],

                // Customer details come from the database profile.
                CustomerName = $"{customer.FirstName} {customer.LastName}".Trim(),
                CustomerEmail = customer.Email,
                CustomerPhone = string.IsNullOrWhiteSpace(customer.Phone)
                    ? null
                    : customer.Phone,

                // Save the checkout address as an order snapshot.
                DeliveryAddress = request.DeliveryAddress.Trim(),
                DeliveryCity = request.DeliveryCity.Trim(),
                DeliveryState = request.DeliveryState.Trim(),
                DeliveryCountry = request.DeliveryCountry.Trim(),
                DeliveryPostalCode = request.DeliveryPostalCode.Trim(),

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

            // Deduct stock and record stock movements.
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
            await orderRepository.SaveChangesAsync(cancellationToken);

            // Return the complete order response.
            return new OrderResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,

                DeliveryAddress = order.DeliveryAddress,
                DeliveryCity = order.DeliveryCity,

                DeliveryState = order.DeliveryState,
                DeliveryCountry = order.DeliveryCountry,
                DeliveryPostalCode = order.DeliveryPostalCode,

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
