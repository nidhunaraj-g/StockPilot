using FluentValidation;
using StockPilot.Application.Features.Suppliers.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Suppliers.Validators
{
    public class UpdateSupplierValidator : AbstractValidator<UpdateSupplierRequest>
    {
        public UpdateSupplierValidator()
        {
            RuleFor(x => x.SupplierName)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.ContactPerson)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(150);

            RuleFor(x => x.Phone)
                .NotEmpty()
                .MaximumLength(20);

            RuleFor(x => x.Address)
                .MaximumLength(250);

            RuleFor(x => x.City)
                .MaximumLength(100);

            RuleFor(x => x.State)
                .MaximumLength(100);

            RuleFor(x => x.Country)
                .MaximumLength(100);
        }
    }
}