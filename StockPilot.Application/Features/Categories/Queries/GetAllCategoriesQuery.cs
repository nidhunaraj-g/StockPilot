using AutoMapper;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Categories.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Categories.Queries
{
    public record GetAllCategoriesQuery() : IRequest<List<CategoryResponse>>;

    public class GetAllCategoriesQueryHandler(IGenericRepository<Category> repository,IMapper mapper)
    : IRequestHandler<GetAllCategoriesQuery, List<CategoryResponse>>
    {
        public async Task<List<CategoryResponse>> Handle(GetAllCategoriesQuery query,CancellationToken cancellationToken)
        {
            var categories = await repository.GetAllAsync(cancellationToken);

            var activeCategories = categories.Where(category => category.IsActive).ToList();

            return mapper.Map<List<CategoryResponse>>(activeCategories);
        }
    }
}
