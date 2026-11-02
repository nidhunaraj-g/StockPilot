using FluentValidation;
using StockPilot.Application.Features.Purchases.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Purchases.Validator
{
    public class ReceivePurchaseValidator : AbstractValidator<ReceivePurchaseRequest>
    {
        public ReceivePurchaseValidator()
        {
            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("At least one purchase item is required.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.PurchaseItemId)
                    .GreaterThan(0)
                    .WithMessage("Invalid purchase item ID.");

                item.RuleFor(x => x.ReceivedQuantity)
                    .GreaterThan(0)
                    .WithMessage("Received quantity must be greater than zero.");
            });

            RuleFor(x => x.Items)
                .Must(items => items == null ||
                    items.Select(item => item.PurchaseItemId).Distinct().Count() == items.Count)
                .WithMessage("Duplicate purchase items are not allowed in one receipt.");
        }
    }
}