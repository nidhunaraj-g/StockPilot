using AutoMapper;
using FluentValidation;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Categories.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Categories.Commands
{
    public record CreateCategoryCommand(CreateCategoryRequest Request): IRequest<CategoryResponse>;

    public class CreateCategoryCommandHandler(IGenericRepository<Category> repository,IMapper mapper,IValidator<CreateCategoryRequest> validator)
        : IRequestHandler<CreateCategoryCommand, CategoryResponse>
    {
        public async Task<CategoryResponse> Handle(CreateCategoryCommand command,CancellationToken cancellationToken)
        {
            // 1. Validate the request.
            await validator.ValidateAndThrowAsync(command.Request,cancellationToken);

            // 2. Normalize the category name.
            var categoryName = command.Request.Name.Trim();

            // 3. Check for a duplicate category name.
            var existingCategory =await repository.FindFirstOrDefaultAsync(category => category.Name == categoryName,cancellationToken);

            if (existingCategory is not null)
            {
                throw new ValidationException("A category with this name already exists.");
            }

            // 4. Map the request DTO to the entity.
            var category = mapper.Map<Category>(command.Request);
            category.Name = categoryName;

            // 5. Add and save the category.
            await repository.AddAsync(category, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            // 6. Return the response DTO.
            return mapper.Map<CategoryResponse>(category);
        }
    }
}
