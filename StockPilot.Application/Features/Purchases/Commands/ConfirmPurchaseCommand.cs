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
    public record ConfirmPurchaseCommand(int Id) : IRequest<bool>;

    public class ConfirmPurchaseCommandHandler(
        IGenericRepository<Purchase> purchaseRepository)
        : IRequestHandler<ConfirmPurchaseCommand, bool>
    {
        public async Task<bool> Handle(
            ConfirmPurchaseCommand command,
            CancellationToken cancellationToken)
        {
            var purchase = await purchaseRepository.GetByIdAsync(
                command.Id, cancellationToken);

            if (purchase is null || !purchase.IsActive)
                return false;

            if (purchase.Status != "Draft")
            {
                throw new ValidationException(
                    "Only draft purchases can be confirmed.");
            }

            purchase.Status = "Ordered";

            purchaseRepository.Update(purchase);

            await purchaseRepository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}