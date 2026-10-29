using AutoMapper;
using FluentValidation;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Brands.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Brands.Commands
{
    public record UpdateBrandCommand(int Id,UpdateBrandRequest Request): IRequest<BrandResponse?>;

    public class UpdateBrandCommandHandler(IGenericRepository<Brand> repository,IMapper mapper,IValidator<UpdateBrandRequest> validator)
        : IRequestHandler<UpdateBrandCommand, BrandResponse?>
    {
        public async Task<BrandResponse?> Handle(UpdateBrandCommand command,CancellationToken cancellationToken)
        {
            // 1. Validate the request
            await validator.ValidateAndThrowAsync(command.Request,cancellationToken);

            // 2. Find the existing brand
            var brand = await repository.GetByIdAsync(command.Id,cancellationToken);

            if (brand is null || !brand.IsActive)
            {
                return null;
            }

            // 3. Normalize the brand name
            var brandName = command.Request.Name.Trim();

            // 4. Check for duplicate names
            var existingBrand = await repository.FindFirstOrDefaultAsync(x => x.Name == brandName && x.Id != command.Id,cancellationToken);

            if (existingBrand is not null)
            {
                throw new ValidationException(
                    "A brand with this name already exists.");
            }

            // 5. Update brand details
            mapper.Map(command.Request, brand);
            brand.Name = brandName;

            // 6. Save changes
            repository.Update(brand);
            await repository.SaveChangesAsync(cancellationToken);

            // 7. Return updated brand
            return mapper.Map<BrandResponse>(brand);
        }
    }
}
