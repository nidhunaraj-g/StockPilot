using FluentValidation;
using StockPilot.Application.Features.Products.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Products.Validators
{
    public class UpdateProductValidator: AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductValidator()
        {
            RuleFor(x => x.SKU)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Barcode)
                .MaximumLength(100);

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.Description)
                .MaximumLength(1000);

            RuleFor(x => x.CategoryId)
                .GreaterThan(0);

            RuleFor(x => x.BrandId)
                .GreaterThan(0)
                .When(x => x.BrandId.HasValue);

            RuleFor(x => x.CostPrice)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.SellingPrice)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.MinimumStockLevel)
                .GreaterThanOrEqualTo(0);
        }
    }
}
