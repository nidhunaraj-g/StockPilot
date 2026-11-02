using MediatR;
using StockPilot.Application.Common.Interfaces;
using StockPilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Purchases.Commands
{
    public record CancelPurchaseCommand(int Id) : IRequest<bool>;

    public class CancelPurchaseCommandHandler(
        IGenericRepository<Purchase> purchaseRepository)
        : IRequestHandler<CancelPurchaseCommand, bool>
    {
        public async Task<bool> Handle(
            CancelPurchaseCommand command,
            CancellationToken cancellationToken)
        {
            var purchase = await purchaseRepository.GetByIdAsync(
                command.Id, cancellationToken);

            if (purchase is null || !purchase.IsActive)
                return false;

            if (purchase.Status != "Draft" &&
                purchase.Status != "Ordered")
            {
                throw new ValidationException(
                    "Only draft or ordered purchases with no received products can be cancelled.");
            }

            purchase.Status = "Cancelled";

            purchaseRepository.Update(purchase);

            await purchaseRepository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}