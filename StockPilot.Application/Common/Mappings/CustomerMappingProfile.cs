using AutoMapper;
using StockPilot.Application.Features.Customers.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Common.Mappings
{
    public class CustomerMappingProfile : Profile
    {
        public CustomerMappingProfile()
        {
            CreateMap<CreateCustomerRequest, Customer>();

            CreateMap<UpdateCustomerRequest, Customer>();

            CreateMap<Customer, CustomerDto>();

        }
    }
}
