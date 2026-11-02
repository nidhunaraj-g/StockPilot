using FluentValidation;
using StockPilot.Application.Features.Purchases.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Purchases.Validator
{
    public class CreatePurchaseValidator : AbstractValidator<CreatePurchaseRequest>
    {
        public CreatePurchaseValidator(IValidator<CreatePurchaseItemRequest> itemValidator)
        {
            RuleFor(x => x.SupplierId)
                .GreaterThan(0)
                .WithMessage("Please select a valid supplier.");

            RuleFor(x => x.ExpectedDeliveryDateUtc)
                .Must(date => !date.HasValue || date.Value >= DateTime.UtcNow.Date)
                .WithMessage("Expected delivery date cannot be in the past.");

            RuleFor(x => x.TaxAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Tax amount cannot be negative.");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("Notes cannot exceed 500 characters.");

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("A purchase must contain at least one item.");

            RuleForEach(x => x.Items)
                .SetValidator(itemValidator);

            RuleFor(x => x.Items)
                .Must(items => items == null ||
                    items.Select(item => item.ProductId).Distinct().Count() == items.Count)
                .WithMessage("Duplicate products are not allowed in a purchase.");
        }
    }
}

