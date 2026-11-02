using AutoMapper;
using FluentValidation;
using MediatR;
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
    public record CreatePurchaseCommand(CreatePurchaseRequest Request)
       : IRequest<PurchaseDto>;

    public class CreatePurchaseCommandHandler(
        IGenericRepository<Purchase> purchaseRepository,
        IGenericRepository<Supplier> supplierRepository,
        IGenericRepository<Product> productRepository,
        IMapper mapper,
        IValidator<CreatePurchaseRequest> validator)
        : IRequestHandler<CreatePurchaseCommand, PurchaseDto>
    {
        public async Task<PurchaseDto> Handle(
            CreatePurchaseCommand command,
            CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(
                command.Request, cancellationToken);

            // Check supplier
            var supplier = await supplierRepository.FindFirstOrDefaultAsync(
                s => s.Id == command.Request.SupplierId && s.IsActive,
                cancellationToken) ?? throw new ValidationException("Supplier not found or inactive.");

            // Create purchase items
            var purchaseItems = new List<PurchaseItem>();

            foreach (var itemRequest in command.Request.Items)
            {
                var product = await productRepository.FindFirstOrDefaultAsync(
                    p => p.Id == itemRequest.ProductId && p.IsActive,
                    cancellationToken) ?? throw new ValidationException(
                        $"Product with ID {itemRequest.ProductId} was not found or is inactive.");
                var item = new PurchaseItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = itemRequest.Quantity,
                    ReceivedQuantity = 0,
                    UnitCost = itemRequest.UnitCost,
                    Subtotal = itemRequest.Quantity * itemRequest.UnitCost
                };

                purchaseItems.Add(item);
            }

            var subtotal = purchaseItems.Sum(item => item.Subtotal);
            var taxAmount = command.Request.TaxAmount;

            // Create purchase
            var purchase = new Purchase
            {
                PurchaseNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20],
                SupplierId = supplier.Id,
                Supplier = supplier,
                PurchaseDateUtc = DateTime.UtcNow,
                ExpectedDeliveryDateUtc = command.Request.ExpectedDeliveryDateUtc,
                Status = "Draft",
                Subtotal = subtotal,
                TaxAmount = taxAmount,
                TotalAmount = subtotal + taxAmount,
                Notes = command.Request.Notes?.Trim(),
                Items = purchaseItems,
                IsActive = true
            };

            await purchaseRepository.AddAsync(purchase, cancellationToken);

            await purchaseRepository.SaveChangesAsync(cancellationToken);

            return mapper.Map<PurchaseDto>(purchase);
        }
    }
}
