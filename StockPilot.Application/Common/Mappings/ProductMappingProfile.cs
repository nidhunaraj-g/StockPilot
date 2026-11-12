using AutoMapper;
using StockPilot.Application.Features.Products.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Common.Mappings
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<ProductImage, ProductImageResponse>();

            CreateMap<Product, ProductResponse>()
                .ForMember(
                    destination => destination.CategoryName,
                    options => options.MapFrom(source =>
                        source.Category.Name))
                .ForMember(
                    destination => destination.BrandName,
                    options => options.MapFrom(source =>
                        source.Brand != null
                            ? source.Brand.Name
                            : null))
                .ForMember(
                    destination => destination.Images,
                    options => options.MapFrom(source =>
                        source.Images.OrderBy(image => image.SortOrder)));
        }
    }
}