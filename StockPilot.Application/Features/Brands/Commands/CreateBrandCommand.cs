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
    public record CreateBrandCommand(CreateBrandRequest Request): IRequest<BrandResponse>;

    public class CreateBrandCommandHandler(IGenericRepository<Brand> repository,IMapper mapper,IValidator<CreateBrandRequest> validator)
        : IRequestHandler<CreateBrandCommand, BrandResponse>
    {
        public async Task<BrandResponse> Handle(CreateBrandCommand command,CancellationToken cancellationToken)
        {
            // 1. Validate the request
            await validator.ValidateAndThrowAsync(command.Request,cancellationToken);

            // 2. Normalize the brand name
            var brandName = command.Request.Name.Trim();

            // 3. Check for duplicate names
            var existingBrand = await repository.FindFirstOrDefaultAsync(brand => brand.Name == brandName,cancellationToken);

            if (existingBrand is not null)
            {
                throw new ValidationException("A brand with this name already exists.");
            }

            // 4. Map request to entity
            var brand = mapper.Map<Brand>(command.Request);

            brand.Name = brandName;
            brand.IsActive = true;

            // 5. Save to database
            await repository.AddAsync(brand, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            // 6. Return response
            return mapper.Map<BrandResponse>(brand);
        }
    }
}
