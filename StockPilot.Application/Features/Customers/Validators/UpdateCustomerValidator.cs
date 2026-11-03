using FluentValidation;
using StockPilot.Application.Features.Customers.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Customers.Validators
{
    public class UpdateCustomerValidator
        : AbstractValidator<UpdateCustomerRequest>
    {
        public UpdateCustomerValidator()
        {
            RuleFor(c => c.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(c => c.LastName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(c => c.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(150);

            RuleFor(c => c.Phone)
                .NotEmpty()
                .MaximumLength(20);

            RuleFor(c => c.Address)
                .MaximumLength(250);

            RuleFor(c => c.City)
                .MaximumLength(100);

            RuleFor(c => c.State)
                .MaximumLength(100);

            RuleFor(c => c.Country)
                .MaximumLength(100);

            RuleFor(c => c.PostalCode)
                .MaximumLength(20);
        }
    }
}