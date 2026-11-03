using FluentValidation;
using StockPilot.Application.Features.Customers.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Customers.Validators
{
    public class CustomerRegisterValidator
        : AbstractValidator<CustomerRegisterRequest>
    {
        public CustomerRegisterValidator()
        {
            RuleFor(c => c.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100);

            RuleFor(c => c.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100);

            RuleFor(c => c.UserName)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(3)
                .MaximumLength(50)
                .Matches("^[a-zA-Z0-9_.-]+$")
                .WithMessage("Username can contain letters, numbers, dots, underscores and hyphens.");

            RuleFor(c => c.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Enter a valid email address.")
                .MaximumLength(150);

            RuleFor(c => c.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain a digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.");

            RuleFor(c => c.ConfirmPassword)
                .Equal(c => c.Password)
                .WithMessage("Passwords do not match.");

            RuleFor(c => c.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20);

            RuleFor(c => c.Address).MaximumLength(250);
            RuleFor(c => c.City).MaximumLength(100);
            RuleFor(c => c.State).MaximumLength(100);
            RuleFor(c => c.Country).MaximumLength(100);
            RuleFor(c => c.PostalCode).MaximumLength(20);
        }
    }
}
