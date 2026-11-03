using FluentValidation;
using StockPilot.Application.Features.Customers.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Customers.Validators
{
    public class CreateCustomerValidator : AbstractValidator<CreateCustomerRequest>
    {
        public CreateCustomerValidator()
        {
            RuleFor(c => c.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100);

            RuleFor(c => c.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100);

            RuleFor(c => c.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Enter a valid email address.")
                .MaximumLength(150);

            RuleFor(c => c.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
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