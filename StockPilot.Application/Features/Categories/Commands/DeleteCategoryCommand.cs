using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Categories.Commands
{
    public record DeleteCategoryCommand(int Id) : IRequest<bool>;

    public class DeleteCategoryCommandHandler(IGenericRepository<Category> repository): IRequestHandler<DeleteCategoryCommand, bool>
    {
        public async Task<bool> Handle(DeleteCategoryCommand command,CancellationToken cancellationToken)
        {
            // Find the category
            var category = await repository.GetByIdAsync(command.Id,cancellationToken);

            // Return false if it doesn't exist
            if (category is null)
            {
                return false;
            }

            // Soft delete: mark the category as inactive
            if (!category.IsActive)
            {
                return true;
            }

            category.IsActive = false;

            // Save changes to the database
            repository.Update(category);

            await repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
