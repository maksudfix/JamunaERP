using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Common;
using JamunaERP.Domain.Entities;
using JamunaERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Dealer> Dealers => Set<Dealer>();
        public DbSet<DealerPayment> DealerPayments => Set<DealerPayment>();
        public DbSet<DealerReturn> DealerReturns => Set<DealerReturn>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
        public DbSet<EmployeePayment> EmployeePayments => Set<EmployeePayment>();
        public DbSet<InstallmentPlan> InstallmentPlans => Set<InstallmentPlan>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleItem> SaleItems => Set<SaleItem>();
        public DbSet<SaleReturn> SaleReturns => Set<SaleReturn>();
        public DbSet<SaleReturnItem> SaleReturnItems => Set<SaleReturnItem>();
        public DbSet<StockItem> StockItems => Set<StockItem>();
        public DbSet<WarrantyService> WarrantyServices => Set<WarrantyService>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. EmployeePayment Relationships with ApplicationUser
            builder.Entity<EmployeePayment>()
                .HasOne<ApplicationUser>()
                .WithMany(u => u.PaymentsReceived)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<EmployeePayment>()
                .HasOne<ApplicationUser>()
                .WithMany(u => u.PaymentsApproved)
                .HasForeignKey(p => p.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Sale Relationships (Auditing / Employee who made the sale)
            builder.Entity<Sale>()
                .HasOne<ApplicationUser>()
                .WithMany(u => u.Sales)
                .HasForeignKey("EmployeeId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // 3. PurchaseOrder Relationships (Auditing / Manager or Employee who placed it)
            builder.Entity<PurchaseOrder>()
                .HasOne<ApplicationUser>()
                .WithMany(u => u.PurchaseOrders)
                .HasForeignKey("CreatedById")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // 4. Precision Configurations for Financial Decimals
            builder.Entity<Product>()
                .Property(p => p.BuyingPrice)
                .HasPrecision(18, 2);

            builder.Entity<Product>()
                .Property(p => p.SellingPrice)
                .HasPrecision(18, 2);

            builder.Entity<Sale>()
                .Property(s => s.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<Sale>()
                .Property(s => s.PaidAmount)
                .HasPrecision(18, 2);

            builder.Entity<Sale>()
                .Property(s => s.ReturnedAmount)
                .HasPrecision(18, 2);

            builder.Entity<Sale>()
                .Property(s => s.RefundedAmount)
                .HasPrecision(18, 2);

            builder.Entity<SaleItem>()
                .Property(si => si.UnitPrice)
                .HasPrecision(18, 2);

            // cost captured at time of sale
            builder.Entity<SaleItem>()
                .Property(si => si.CostPrice)
                .HasPrecision(18, 2);

            // SaleItem to StockItem relationship configuration
            builder.Entity<SaleItem>()
                .HasOne(x => x.StockItem)
                .WithMany()
                .HasForeignKey(x => x.StockItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // purchase cost per unit
            builder.Entity<PurchaseOrderItem>()
                .Property(pi => pi.UnitCost)
                .HasPrecision(18, 2);

            builder.Entity<InstallmentPlan>()
                .Property(i => i.AmountDue)
                .HasPrecision(18, 2);

            builder.Entity<InstallmentPlan>()
                .Property(i => i.AmountPaid)
                .HasPrecision(18, 2);

            builder.Entity<EmployeePayment>()
                .Property(ep => ep.BaseSalary)
                .HasPrecision(18, 2);

            builder.Entity<EmployeePayment>()
                .Property(ep => ep.Bonus)
                .HasPrecision(18, 2);

            builder.Entity<EmployeePayment>()
                .Property(ep => ep.Deductions)
                .HasPrecision(18, 2);

            builder.Entity<EmployeePayment>()
                .Property(ep => ep.NetPaidAmount)
                .HasPrecision(18, 2);

            // 5. Concurrency Tokens
            builder.Entity<StockItem>()
                .Property(s => s.RowVersion)
                .IsRowVersion();

            builder.Entity<Sale>()
                .Property(s => s.RowVersion)
                .IsRowVersion();

            builder.Entity<PurchaseOrder>()
                .Property(p => p.RowVersion)
                .IsRowVersion();

            // 6. String lengths (SQL Server cannot index nvarchar(max) columns)
            builder.Entity<Sale>()
                .Property(s => s.InvoiceNo)
                .HasMaxLength(50);

            builder.Entity<PurchaseOrder>()
                .Property(p => p.DealerInvoiceNo)
                .HasMaxLength(50);

            builder.Entity<StockItem>()
                .Property(s => s.SerialNumber)
                .HasMaxLength(100);

            // 7. Unique indexes
            builder.Entity<Sale>()
                .HasIndex(s => s.InvoiceNo)
                .IsUnique();

            builder.Entity<PurchaseOrder>()
                .HasIndex(p => new { p.DealerId, p.DealerInvoiceNo })
                .IsUnique();

            builder.Entity<StockItem>()
                .HasIndex(s => s.SerialNumber)
                .IsUnique()
                .HasFilter("[SerialNumber] IS NOT NULL");

            builder.Entity<InstallmentPlan>()
                .HasIndex(i => new { i.SaleId, i.InstallmentNumber })
                .IsUnique();

            // One payroll record per employee per month
            builder.Entity<EmployeePayment>()
                .HasIndex(p => new { p.EmployeeId, p.PaymentMonth, p.PaymentYear })
                .IsUnique();

            // 8. Audit column lengths
            foreach (var clrType in builder.Model.GetEntityTypes()
                                           .Select(t => t.ClrType)
                                           .Where(t => typeof(BaseEntity).IsAssignableFrom(t))
                                           .ToList())
            {
                builder.Entity(clrType).Property(nameof(BaseEntity.CreatedBy)).HasMaxLength(450);
                builder.Entity(clrType).Property(nameof(BaseEntity.UpdatedBy)).HasMaxLength(450);
            }

            // 9. Soft-delete flags (existing rows default to active)
            builder.Entity<Customer>().Property(x => x.IsActive).HasDefaultValue(true);
            builder.Entity<Dealer>().Property(x => x.IsActive).HasDefaultValue(true);
            builder.Entity<Product>().Property(x => x.IsActive).HasDefaultValue(true);

            // 10. Sale returns
            builder.Entity<SaleReturn>()
                .HasOne(r => r.Sale)
                .WithMany(s => s.Returns)
                .HasForeignKey(r => r.SaleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SaleReturnItem>()
                .HasOne(i => i.SaleReturn)
                .WithMany(r => r.Items)
                .HasForeignKey(i => i.SaleReturnId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict avoids SQL Server "multiple cascade paths" (Sale -> SaleItem -> SaleReturnItem)
            builder.Entity<SaleReturnItem>()
                .HasOne(i => i.SaleItem)
                .WithMany()
                .HasForeignKey(i => i.SaleItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SaleReturn>().Property(r => r.ReturnNo).HasMaxLength(50);
            builder.Entity<SaleReturn>().Property(r => r.Reason).HasMaxLength(300);
            builder.Entity<SaleReturn>().HasIndex(r => r.ReturnNo).IsUnique();
            builder.Entity<SaleReturn>().Property(r => r.ReturnedValue).HasPrecision(18, 2);
            builder.Entity<SaleReturn>().Property(r => r.OutstandingReduced).HasPrecision(18, 2);
            builder.Entity<SaleReturn>().Property(r => r.RefundAmount).HasPrecision(18, 2);
            builder.Entity<SaleReturnItem>().Property(i => i.UnitPrice).HasPrecision(18, 2);

            // 11. Sale payments
            builder.Entity<Payment>(e =>
            {
                e.Property(p => p.Amount).HasPrecision(18, 2);
                e.Property(p => p.Method).HasMaxLength(30);
                e.Property(p => p.ReceiptNo).HasMaxLength(50);
                e.HasIndex(p => p.SaleId);
                e.HasOne(p => p.Sale)
                    .WithMany(s => s.Payments)
                    .HasForeignKey(p => p.SaleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 12. Dealer payments
            builder.Entity<DealerPayment>(e =>
            {
                e.Property(p => p.Amount).HasPrecision(18, 2);
                e.Property(p => p.Notes).HasMaxLength(500);
                e.HasIndex(p => p.DealerId);
                e.HasOne(dp => dp.Dealer)
                    .WithMany(d => d.Payments)
                    .HasForeignKey(dp => dp.DealerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<Sale>().Property(s => s.DiscountAmount).HasPrecision(18, 2);

            builder.Entity<Expense>(e =>
            {
                e.Property(x => x.Title).HasMaxLength(150);
                e.Property(x => x.Category).HasMaxLength(50);
                e.Property(x => x.Notes).HasMaxLength(300);
                e.Property(x => x.Amount).HasPrecision(18, 2);
                e.HasIndex(x => x.ExpenseDate);
            });

            builder.Entity<StockAdjustment>(e =>
            {
                e.Property(x => x.Reason).HasMaxLength(300);
                e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<DealerReturn>(e =>
            {
                e.Property(x => x.Reason).HasMaxLength(300);
                e.Property(x => x.CreditAmount).HasPrecision(18, 2);
                e.HasOne(x => x.Dealer).WithMany().HasForeignKey(x => x.DealerId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}