using FluentValidation;
using StockPilot.Application.Features.Purchases.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Purchases.Validator
{
    public class CreatePurchaseItemValidator : AbstractValidator<CreatePurchaseItemRequest>
    {
        public CreatePurchaseItemValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithMessage("Please select a valid product.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Purchase quantity must be greater than zero.");

            RuleFor(x => x.UnitCost)
                .GreaterThan(0)
                .WithMessage("Unit cost must be greater than zero.");
        }
    }
}
