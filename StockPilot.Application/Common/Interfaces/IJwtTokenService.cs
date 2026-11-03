using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Common.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(
            int userId,
            string userName,
            string email,
            int? customerId = null,
            IEnumerable<string>? roles = null);
    }
}
