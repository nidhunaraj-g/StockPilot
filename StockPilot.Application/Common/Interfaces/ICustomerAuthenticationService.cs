using StockPilot.Application.Features.Customers.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Common.Interfaces
{
    public interface ICustomerAuthenticationService
    {
        Task<CustomerLoginResponse> LoginAsync(
            CustomerLoginRequest request,
            CancellationToken cancellationToken = default);
    }
}
