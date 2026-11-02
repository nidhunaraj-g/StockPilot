using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Purchases.DTOs;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Purchases.Commands
{
    public record ReceivePurchaseCommand(
        int PurchaseId,
        ReceivePurchaseRequest Request
    ) : IRequest<bool>;

    public class ReceivePurchaseCommandHandler(
        IGenericRepository<Purchase> purchaseRepository,
        IGenericRepository<Product> productRepository,
        IGenericRepository<StockMovement> movementRepository,
        IValidator<ReceivePurchaseRequest> validator)
        : IRequestHandler<ReceivePurchaseCommand, bool>
    {
        public async Task<bool> Handle(
            ReceivePurchaseCommand command,
            CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(
                command.Request, cancellationToken);

            var purchase = await purchaseRepository
                .GetQueryable()
                .Include(p => p.Items)
                .FirstOrDefaultAsync(
                    p => p.Id == command.PurchaseId && p.IsActive,
                    cancellationToken);

            if (purchase is null)
                return false;

            if (purchase.Status != "Ordered" &&
                purchase.Status != "PartiallyReceived")
            {
                throw new ValidationException(
                    "Only ordered or partially received purchases can receive products.");
            }

            foreach (var requestItem in command.Request.Items)
            {
                var purchaseItem = purchase.Items.FirstOrDefault(
                    i => i.Id == requestItem.PurchaseItemId) ?? throw new ValidationException(
                        $"Purchase item {requestItem.PurchaseItemId} was not found in this purchase.");
                if (requestItem.ReceivedQuantity >
                    purchaseItem.Quantity - purchaseItem.ReceivedQuantity)
                {
                    throw new ValidationException(
                        $"Cannot receive more than the pending quantity for product ID {purchaseItem.ProductId}.");
                }

                var product = await productRepository.GetByIdAsync(
                    purchaseItem.ProductId, cancellationToken);

                if (product is null || !product.IsActive)
                {
                    throw new ValidationException(
                        $"Active product with ID {purchaseItem.ProductId} was not found.");
                }

                if (product.StockQuantity >
                    int.MaxValue - requestItem.ReceivedQuantity)
                {
                    throw new ValidationException(
                        "Stock quantity exceeds the allowed limit.");
                }

                // Increase received quantity
                purchaseItem.ReceivedQuantity +=
                    requestItem.ReceivedQuantity;

                // Increase available stock
                product.StockQuantity += requestItem.ReceivedQuantity;

                // Record stock movement
                var movement = new StockMovement
                {
                    ProductId = product.Id,
                    Quantity = requestItem.ReceivedQuantity,
                    MovementType = "IN",
                    Reason = $"Purchase receipt - {purchase.PurchaseNumber}"
                };

                productRepository.Update(product);

                await movementRepository.AddAsync(
                    movement, cancellationToken);
            }

            // Check whether all purchase items are received
            var allReceived = purchase.Items.All(
                item => item.ReceivedQuantity == item.Quantity);

            purchase.Status = allReceived
                ? "Received"
                : "PartiallyReceived";

            if (allReceived)
            {
                purchase.ReceivedDateUtc = DateTime.UtcNow;
            }

            purchaseRepository.Update(purchase);

            // All changes are saved together through the shared DbContext.
            // SQL Server RowVersion detects conflicting product updates.
            // A single SaveChanges call persists the changes atomically.
            await purchaseRepository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}