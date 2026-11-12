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
    public record GetMyOrdersQuery(
        int CustomerId,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<MyOrderListResponse>;

    public class MyOrderListResponse
    {
        public List<OrderResponse> Orders { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class GetMyOrdersQueryHandler(
        IGenericRepository<Order> orderRepository)
        : IRequestHandler<GetMyOrdersQuery, MyOrderListResponse>
    {
        public async Task<MyOrderListResponse> Handle(
            GetMyOrdersQuery query,
            CancellationToken cancellationToken)
        {
            if (query.CustomerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero.");

            if (query.PageNumber < 1)
                throw new ArgumentException("Page number must be at least 1.");

            if (query.PageSize < 1 || query.PageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100.");

            // Retrieve only orders belonging to this customer.
            var ordersQuery = orderRepository
                .GetQueryable()
                .Where(o => o.CustomerId == query.CustomerId)
                .Include(o => o.Items)
                    .ThenInclude(item => item.Product);

            var totalCount = await ordersQuery.CountAsync(cancellationToken);

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDateUtc)
                .ThenByDescending(o => o.Id)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            var orderResponses = orders.Select(order => new OrderResponse
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
            }).ToList();

            return new MyOrderListResponse
            {
                Orders = orderResponses,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(
                    totalCount / (double)query.PageSize)
            };
        }
    }
}
