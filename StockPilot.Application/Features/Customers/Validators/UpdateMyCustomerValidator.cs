using FluentValidation;
using StockPilot.Application.Features.Customers.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Customers.Validators
{
    public class UpdateMyCustomerValidator
        : AbstractValidator<UpdateMyCustomerRequest>
    {
        public UpdateMyCustomerValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

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

            RuleFor(x => x.PostalCode)
                .MaximumLength(20);
        }
    }
}