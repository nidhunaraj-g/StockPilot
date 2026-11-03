using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Customers.DTOs
{
    public class CustomerLoginRequest
    {
        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
