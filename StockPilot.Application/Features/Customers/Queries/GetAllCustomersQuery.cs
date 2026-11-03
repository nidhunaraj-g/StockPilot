using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    public record GetAllCustomersQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? Search = null)
        : IRequest<CustomerListResponse>;

    public class GetAllCustomersQueryHandler(
        IGenericRepository<Customer> repository,
        IMapper mapper)
        : IRequestHandler<GetAllCustomersQuery, CustomerListResponse>
    {
        public async Task<CustomerListResponse> Handle(
            GetAllCustomersQuery query,
            CancellationToken cancellationToken)
        {
            if (query.PageNumber < 1)
                throw new ArgumentException("Page number must be at least 1.");

            if (query.PageSize < 1 || query.PageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100.");

            var customersQuery = repository.GetQueryable()
                .Where(c => c.IsActive);

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
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ProjectTo<CustomerDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new CustomerListResponse
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
