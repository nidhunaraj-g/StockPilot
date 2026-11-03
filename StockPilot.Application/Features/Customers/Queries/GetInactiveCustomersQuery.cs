using MediatR;
using Microsoft.EntityFrameworkCore;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Customers.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Customers.Queries
{
    public record GetInactiveCustomersQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? Search = null)
        : IRequest<InactiveCustomerListResponse>;

    public class GetInactiveCustomersQueryHandler(
        IGenericRepository<Customer> repository)
        : IRequestHandler<GetInactiveCustomersQuery, InactiveCustomerListResponse>
    {
        public async Task<InactiveCustomerListResponse> Handle(
            GetInactiveCustomersQuery query,
            CancellationToken cancellationToken)
        {
            if (query.PageNumber < 1)
                throw new ArgumentException(
                    "Page number must be at least 1.");

            if (query.PageSize < 1 || query.PageSize > 100)
                throw new ArgumentException(
                    "Page size must be between 1 and 100.");

            var customersQuery = repository.GetQueryable()
                .Where(c => !c.IsActive);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                customersQuery = customersQuery.Where(c =>
                    c.FirstName.Contains(search) ||
                    c.LastName.Contains(search) ||
                    c.Email.Contains(search) ||
                    c.Phone.Contains(search));
            }

            var totalCount = await customersQuery.CountAsync(
                cancellationToken);

            var customers = await customersQuery
                .OrderByDescending(c => c.DeactivatedAtUtc)
                .ThenBy(c => c.FirstName)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new InactiveCustomerDto
                {
                    Id = c.Id,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    Phone = c.Phone,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    Country = c.Country,
                    PostalCode = c.PostalCode,
                    DeactivatedAtUtc = c.DeactivatedAtUtc,
                    IsAdminDeactivated = c.IsAdminDeactivated,
                    CreatedAtUtc = c.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);

            return new InactiveCustomerListResponse
            {
                Customers = customers,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(
                    totalCount / (double)query.PageSize)
            };
        }
    }
}