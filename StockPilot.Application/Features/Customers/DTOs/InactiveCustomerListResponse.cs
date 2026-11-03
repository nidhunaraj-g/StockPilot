using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.Application.Features.Customers.DTOs
{
    public class InactiveCustomerListResponse
    {
        public List<InactiveCustomerDto> Customers { get; set; } = [];

        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}