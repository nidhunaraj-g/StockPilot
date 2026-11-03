using FluentValidation;
using StockPilot.Application.Features.Customers.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Customers.Validators
{
    public class CustomerLoginValidator
       : AbstractValidator<CustomerLoginRequest>
    {
        public CustomerLoginValidator()
        {
            RuleFor(c => c.UserName)
                .NotEmpty()
                .WithMessage("Username is required.");

            RuleFor(c => c.Password)
                .NotEmpty()
                .WithMessage("Password is required.");
        }
    }
}
