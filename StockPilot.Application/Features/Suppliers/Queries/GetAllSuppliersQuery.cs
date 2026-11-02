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
    public record GetAllSuppliersQuery : IRequest<List<SupplierDto>>;

    public class GetAllSuppliersQueryHandler(IGenericRepository<Supplier> repository,IMapper mapper) 
        : IRequestHandler<GetAllSuppliersQuery, List<SupplierDto>>
    {
        public async Task<List<SupplierDto>> Handle(GetAllSuppliersQuery query,CancellationToken cancellationToken)
        {
            var suppliers = await repository.GetAllAsync(cancellationToken);

            var activeSuppliers = suppliers
                .Where(supplier => supplier.IsActive)
                .ToList();

            return mapper.Map<List<SupplierDto>>(activeSuppliers);
        }
    }
}

