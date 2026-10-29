using Microsoft.AspNetCore.Identity;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;

namespace JamunaERP.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public StockLocationType AssignedLocation { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<EmployeePayment> PaymentsReceived { get; set; } = new List<EmployeePayment>();
    public ICollection<EmployeePayment> PaymentsApproved { get; set; } = new List<EmployeePayment>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}