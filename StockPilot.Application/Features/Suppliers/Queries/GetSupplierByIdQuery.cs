using AutoMapper;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Suppliers.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Suppliers.Queries
{
    public record GetSupplierByIdQuery(int Id) : IRequest<SupplierDto?>;
    public class GetSupplierByIdQueryHandler(IGenericRepository<Supplier> repository,IMapper mapper) 
        : IRequestHandler<GetSupplierByIdQuery, SupplierDto?>
    {
        public async Task<SupplierDto?> Handle(GetSupplierByIdQuery query,CancellationToken cancellationToken)
        {
            var supplier = await repository.GetByIdAsync(query.Id,cancellationToken);

            if (supplier is null || !supplier.IsActive)
            {
                return null;
            }

            return mapper.Map<SupplierDto>(supplier);
        }
    }
}