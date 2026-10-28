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
    public record UpdateCategoryCommand(int Id,UpdateCategoryRequest Request): IRequest<CategoryResponse?>;

    public class UpdateCategoryCommandHandler(IGenericRepository<Category> repository,IMapper mapper,IValidator<UpdateCategoryRequest> validator)
        : IRequestHandler<UpdateCategoryCommand, CategoryResponse?>
    {
        public async Task<CategoryResponse?> Handle(UpdateCategoryCommand command,CancellationToken cancellationToken)
        {
            // 1. Validate the request
            await validator.ValidateAndThrowAsync(command.Request,cancellationToken);

            // 2. Find the existing category
            var category = await repository.GetByIdAsync(command.Id,cancellationToken);

            if (category is null)
            {
                return null;
            }

            // 3. Check for duplicate category names
            var categoryName = command.Request.Name.Trim();

            var existingCategory =
                await repository.FindFirstOrDefaultAsync(
                    x => x.Name == categoryName &&
                         x.Id != command.Id,
                    cancellationToken);

            if (existingCategory is not null)
            {
                throw new ValidationException(
                    "A category with this name already exists.");
            }

            // 4. Update category properties
            mapper.Map(command.Request, category);
            category.Name = categoryName;

            // 5. Save changes
            repository.Update(category);

            await repository.SaveChangesAsync(cancellationToken);

            // 6. Return the updated category
            return mapper.Map<CategoryResponse>(category);
        }
    }
}
