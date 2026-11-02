
using AutoMapper;
using FluentValidation;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Suppliers.DTOs;
using StockPilot.Domain.Entities;

namespace StockPilot.Application.Features.Suppliers.Commands
{
    public record CreateSupplierCommand(
        CreateSupplierRequest Request
    ) : IRequest<SupplierDto>;

    public class CreateSupplierCommandHandler(
        IGenericRepository<Supplier> repository,
        IMapper mapper,
        IValidator<CreateSupplierRequest> validator
    ) : IRequestHandler<CreateSupplierCommand, SupplierDto>
    {
        public async Task<SupplierDto> Handle(
            CreateSupplierCommand command,
            CancellationToken cancellationToken)
        {
            // Validate request
            await validator.ValidateAndThrowAsync(
                command.Request,
                cancellationToken);

            var supplierName =
                command.Request.SupplierName.Trim();

            var email = command.Request.Email
                .Trim()
                .ToLowerInvariant();

            // Check duplicate email
            var existingSupplier =
                await repository.FindFirstOrDefaultAsync(
                    supplier => supplier.Email == email,
                    cancellationToken);

            if (existingSupplier is not null)
            {
                throw new ValidationException(
                    "A supplier with this email already exists.");
            }

            // Map request to entity
            var supplier = mapper.Map<Supplier>(command.Request);

            supplier.SupplierName = supplierName;
            supplier.Email = email;
            supplier.IsActive = true;
            supplier.CreatedAtUtc = DateTime.UtcNow;

            // Save supplier
            await repository.AddAsync(
                supplier,
                cancellationToken);

            await repository.SaveChangesAsync(
                cancellationToken);

            return mapper.Map<SupplierDto>(supplier);
        }
    }
}
