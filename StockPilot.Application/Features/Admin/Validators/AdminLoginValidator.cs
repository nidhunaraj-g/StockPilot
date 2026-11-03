using FluentValidation;
using StockPilot.Application.Features.Admin.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Admin.Validators
{
    public class AdminLoginValidator : AbstractValidator<AdminLoginRequest>
    {
        public AdminLoginValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
