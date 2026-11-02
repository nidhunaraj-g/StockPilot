
using AutoMapper;
using FluentValidation;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Suppliers.DTOs;
using StockPilot.Domain.Entities;

namespace StockPilot.Application.Features.Suppliers.Commands
{
    public record UpdateSupplierCommand(
        int Id,
        UpdateSupplierRequest Request
    ) : IRequest<SupplierDto?>;

    public class UpdateSupplierCommandHandler(
        IGenericRepository<Supplier> repository,
        IMapper mapper,
        IValidator<UpdateSupplierRequest> validator
    ) : IRequestHandler<UpdateSupplierCommand, SupplierDto?>
    {
        public async Task<SupplierDto?> Handle(
            UpdateSupplierCommand command,
            CancellationToken cancellationToken)
        {
            // Validate request
            await validator.ValidateAndThrowAsync(
                command.Request,
                cancellationToken);

            // Find supplier
            var supplier = await repository.GetByIdAsync(
                command.Id,
                cancellationToken);

            if (supplier is null || !supplier.IsActive)
            {
                return null;
            }

            // Normalize email
            var email = command.Request.Email
                .Trim()
                .ToLowerInvariant();

            // Check duplicate email
            var existingSupplier =
                await repository.FindFirstOrDefaultAsync(
                    s => s.Email == email && s.Id != command.Id,
                    cancellationToken);

            if (existingSupplier is not null)
            {
                throw new ValidationException(
                    "Another supplier with this email already exists.");
            }

            // Update supplier properties
            mapper.Map(command.Request, supplier);

            supplier.SupplierName =
                command.Request.SupplierName.Trim();

            supplier.Email = email;
            supplier.UpdatedAtUtc = DateTime.UtcNow;

            // Save changes
            repository.Update(supplier);

            await repository.SaveChangesAsync(
                cancellationToken);

            // Return updated supplier
            return mapper.Map<SupplierDto>(supplier);
        }
    }
}
