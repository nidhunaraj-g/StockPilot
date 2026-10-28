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
    public record GetCategoryByIdQuery(int Id): IRequest<CategoryResponse?>;
    public class GetCategoryByIdQueryHandler(IGenericRepository<Category> repository,IMapper mapper)
        : IRequestHandler<GetCategoryByIdQuery, CategoryResponse?>
    {
        public async Task<CategoryResponse?> Handle(GetCategoryByIdQuery query,CancellationToken cancellationToken)
        {
            var category = await repository.GetByIdAsync(query.Id,cancellationToken);

            // Return null if the category doesn't exist or is inactive.
            if (category is null || !category.IsActive)
            {
                return null;
            }

            return mapper.Map<CategoryResponse>(category);
        }
    }
}
