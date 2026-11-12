using MediatR;
using Microsoft.EntityFrameworkCore;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Orders.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Orders.Queries
{
    public record GetMyOrderByIdQuery(
        int OrderId,
        int CustomerId
    ) : IRequest<OrderResponse?>;

    public class GetMyOrderByIdQueryHandler(
        IGenericRepository<Order> orderRepository)
        : IRequestHandler<GetMyOrderByIdQuery, OrderResponse?>
    {
        public async Task<OrderResponse?> Handle(
            GetMyOrderByIdQuery query,
            CancellationToken cancellationToken)
        {
            if (query.OrderId <= 0)
                throw new ArgumentException("Order ID must be greater than zero.");

            if (query.CustomerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.");

            // Retrieve the order only if it belongs to the logged-in customer.
            var order = await orderRepository
                .GetQueryable()
                .Include(o => o.Items)
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(
                    o => o.Id == query.OrderId &&
                         o.CustomerId == query.CustomerId,
                    cancellationToken);

            if (order is null)
                return null;

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

                Items = [.. order.Items.Select(item => new OrderItemResponse
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal
                })]
            };
        }
    }
}