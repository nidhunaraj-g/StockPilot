using StockPilot.Application.Features.Admin.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Common.Interfaces
{
    public interface IAdminAuthenticationService
    {
        Task<AdminLoginResponse> LoginAsync(
            AdminLoginRequest request,
            CancellationToken cancellationToken = default);
    }
}
