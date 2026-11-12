using System;
using System.Collections.Generic;

namespace StockPilot.Domain.Entities
{
    public class Customer : AuditableEntity
    {
        public int ApplicationUserId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;

        public ICollection<Order> Orders { get; set; } = [];

        public DateTime? DeactivatedAtUtc { get; set; }
        public bool IsAdminDeactivated { get; set; } = false;
    }
}
