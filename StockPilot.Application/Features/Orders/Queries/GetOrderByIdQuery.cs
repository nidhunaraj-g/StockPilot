using MediatR;
using Microsoft.EntityFrameworkCore;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Orders.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Orders.Queries
{
    public record GetOrderByIdQuery(int Id) : IRequest<OrderResponse?>;

    public class GetOrderByIdQueryHandler(
        IGenericRepository<Order> orderRepository)
        : IRequestHandler<GetOrderByIdQuery, OrderResponse?>
    {
        public async Task<OrderResponse?> Handle(
            GetOrderByIdQuery query,
            CancellationToken cancellationToken)
        {
            if (query.Id <= 0)
                throw new ArgumentException("Order ID must be greater than zero.");

            var order = await orderRepository
                .GetQueryable()
                .Include(o => o.Items)
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(
                    o => o.Id == query.Id,
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
