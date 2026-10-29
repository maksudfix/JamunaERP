using JamunaERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace JamunaERP.Application.Common.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<Customer> Customers { get; }
        DbSet<Dealer> Dealers { get; }
        DbSet<DealerPayment> DealerPayments { get; }
        DbSet<EmployeePayment> EmployeePayments { get; }
        DbSet<InstallmentPlan> InstallmentPlans { get; }
        DbSet<Payment> Payments { get; }
        DbSet<Product> Products { get; }
        DbSet<PurchaseOrder> PurchaseOrders { get; }
        DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }
        DbSet<Sale> Sales { get; }
        DbSet<SaleItem> SaleItems { get; }
        DbSet<DealerReturn> DealerReturns { get; }
        DbSet<Expense> Expenses { get; }
        DbSet<StockAdjustment> StockAdjustments { get; }
        DbSet<SaleReturn> SaleReturns { get; }
        DbSet<SaleReturnItem> SaleReturnItems { get; }
        DbSet<StockItem> StockItems { get; }
        DbSet<WarrantyService> WarrantyServices { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}