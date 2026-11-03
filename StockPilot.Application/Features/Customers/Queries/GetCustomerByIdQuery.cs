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
    public record GetCustomerByIdQuery(int Id)
        : IRequest<CustomerDto?>;

    public class GetCustomerByIdQueryHandler(
        IGenericRepository<Customer> repository,
        IMapper mapper)
        : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
    {
        public async Task<CustomerDto?> Handle(
            GetCustomerByIdQuery query,
            CancellationToken cancellationToken)
        {
            var customer = await repository.GetByIdAsync(
                query.Id,
                cancellationToken);

            if (customer is null || !customer.IsActive)
                return null;

            return mapper.Map<CustomerDto>(customer);
        }
    }
}

