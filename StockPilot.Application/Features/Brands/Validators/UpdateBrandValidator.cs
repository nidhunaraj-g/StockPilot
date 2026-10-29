using FluentValidation;
using StockPilot.Application.Features.Brands.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Brands.Validators
{
    public class UpdateBrandValidator : AbstractValidator<UpdateBrandRequest>
    {
        public UpdateBrandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Brand name is required.")
                .MaximumLength(100)
                .WithMessage("Brand name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.");

            RuleFor(x => x.LogoUrl)
                .MaximumLength(500)
                .WithMessage("Logo URL cannot exceed 500 characters.");
        }
    }
}
