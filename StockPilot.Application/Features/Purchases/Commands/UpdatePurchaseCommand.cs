
using FluentValidation;
using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Application.Features.Purchases.DTOs;
using StockPilot.Domain.Entities;

namespace StockPilot.Application.Features.Purchases.Commands
{
    public record UpdatePurchaseCommand(int Id, CreatePurchaseRequest Request)
        : IRequest<PurchaseDto?>;

    public class UpdatePurchaseCommandHandler(
        IGenericRepository<Purchase> purchaseRepository,
        IGenericRepository<Supplier> supplierRepository,
        IGenericRepository<Product> productRepository,
        IValidator<CreatePurchaseRequest> validator)
        : IRequestHandler<UpdatePurchaseCommand, PurchaseDto?>
    {
        public async Task<PurchaseDto?> Handle(
            UpdatePurchaseCommand command,
            CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(
                command.Request, cancellationToken);

            // Load the purchase and its items with tracking enabled.
            var purchases =
                await purchaseRepository.GetAllWithIncludesTrackedAsync(
                    cancellationToken,
                    p => p.Supplier,
                    p => p.Items);

            var existingPurchase = purchases.FirstOrDefault(
                p => p.Id == command.Id && p.IsActive);

            if (existingPurchase is null)
                return null;

            if (existingPurchase.Status != "Draft")
            {
                throw new ValidationException(
                    "Only draft purchases can be updated.");
            }

            var supplier = await supplierRepository.FindFirstOrDefaultAsync(
                s => s.Id == command.Request.SupplierId && s.IsActive,
                cancellationToken);

            if (supplier is null)
            {
                throw new ValidationException(
                    "Supplier not found or inactive.");
            }

            var updatedItems = new List<PurchaseItem>();

            foreach (var itemRequest in command.Request.Items)
            {
                var product = await productRepository.FindFirstOrDefaultAsync(
                    p => p.Id == itemRequest.ProductId && p.IsActive,
                    cancellationToken);

                if (product is null)
                {
                    throw new ValidationException(
                        $"Product with ID {itemRequest.ProductId} was not found or is inactive.");
                }

                updatedItems.Add(new PurchaseItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = itemRequest.Quantity,
                    ReceivedQuantity = 0,
                    UnitCost = itemRequest.UnitCost,
                    Subtotal = itemRequest.Quantity * itemRequest.UnitCost
                });
            }

            // Update the supplier foreign key.
            // Do not replace the already-tracked Supplier navigation.
            existingPurchase.SupplierId = supplier.Id;

            existingPurchase.ExpectedDeliveryDateUtc =
                command.Request.ExpectedDeliveryDateUtc;

            existingPurchase.Notes = command.Request.Notes?.Trim();

            // Remove old items from the tracked collection.
            // EF Core marks required orphaned items for deletion.
            existingPurchase.Items.Clear();

            // Add the replacement items.
            foreach (var item in updatedItems)
            {
                existingPurchase.Items.Add(item);
            }

            existingPurchase.Subtotal =
                updatedItems.Sum(item => item.Subtotal);

            existingPurchase.TaxAmount = command.Request.TaxAmount;

            existingPurchase.TotalAmount =
                existingPurchase.Subtotal + existingPurchase.TaxAmount;

            existingPurchase.UpdatedAtUtc = DateTime.UtcNow;

            // Save tracked changes, including old item deletions
            // and new item insertions.
            await purchaseRepository.SaveChangesAsync(cancellationToken);

            // New item IDs are now populated by EF Core.
            return new PurchaseDto
            {
                Id = existingPurchase.Id,
                PurchaseNumber = existingPurchase.PurchaseNumber,
                SupplierId = existingPurchase.SupplierId,
                SupplierName = supplier.SupplierName,
                PurchaseDateUtc = existingPurchase.PurchaseDateUtc,
                ExpectedDeliveryDateUtc =
                    existingPurchase.ExpectedDeliveryDateUtc,
                ReceivedDateUtc = existingPurchase.ReceivedDateUtc,
                Status = existingPurchase.Status,
                Subtotal = existingPurchase.Subtotal,
                TaxAmount = existingPurchase.TaxAmount,
                TotalAmount = existingPurchase.TotalAmount,
                Notes = existingPurchase.Notes,

                Items = [.. updatedItems.Select(item =>
                    new PurchaseItemDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        ReceivedQuantity = item.ReceivedQuantity,
                        UnitCost = item.UnitCost,
                        Subtotal = item.Subtotal
                    })]
            };
        }
    }
}
