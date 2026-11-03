using AutoMapper;
using MediatR;
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
    public record GetMyCustomerProfileQuery(int UserId,int CustomerId) : IRequest<CustomerDto?>;

    public class GetMyCustomerProfileQueryHandler(IGenericRepository<Customer> repository,IMapper mapper)
        : IRequestHandler<GetMyCustomerProfileQuery, CustomerDto?>
    {
        public async Task<CustomerDto?> Handle(
            GetMyCustomerProfileQuery query,
            CancellationToken cancellationToken)
        {
            var customer = await repository.FindFirstOrDefaultAsync(
                c => c.Id == query.CustomerId &&
                     c.ApplicationUserId == query.UserId &&
                     c.IsActive,
                cancellationToken);

            if (customer is null)
                return null;

            return mapper.Map<CustomerDto>(customer);
        }
    }
}
