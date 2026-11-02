using AutoMapper;
using StockPilot.Application.Features.Suppliers.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Common.Mappings
{
    public class SupplierMappingProfile : Profile
    {
        public SupplierMappingProfile()
        {
            CreateMap<CreateSupplierRequest, Supplier>();

            CreateMap<UpdateSupplierRequest, Supplier>();

            CreateMap<Supplier, SupplierDto>();
        }
    }
}
