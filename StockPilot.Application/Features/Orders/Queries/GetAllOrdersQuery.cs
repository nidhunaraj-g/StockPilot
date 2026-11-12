using MediatR;
using Microsoft.EntityFrameworkCore;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Orders.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Orders.Queries
{
    public record GetAllOrdersQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? Search = null,
        string? Status = null
    ) : IRequest<OrderListResponse>;

    public class OrderListResponse
    {
        public List<OrderResponse> Orders { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class GetAllOrdersQueryHandler(
        IGenericRepository<Order> orderRepository)
        : IRequestHandler<GetAllOrdersQuery, OrderListResponse>
    {
        public async Task<OrderListResponse> Handle(
            GetAllOrdersQuery query,
            CancellationToken cancellationToken)
        {
            if (query.PageNumber < 1)
                throw new ArgumentException("Page number must be at least 1.");

            if (query.PageSize < 1 || query.PageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100.");

            var ordersQuery = orderRepository
                .GetQueryable()
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                ordersQuery = ordersQuery.Where(o =>
                    o.OrderNumber.Contains(search) ||
                    o.CustomerName.Contains(search) ||
                    o.CustomerEmail.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var status = query.Status.Trim();

                ordersQuery = ordersQuery.Where(o => o.Status == status);
            }

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

            return new OrderListResponse
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
