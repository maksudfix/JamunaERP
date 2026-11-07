using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Common;
using JamunaERP.Domain.Entities;
using JamunaERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options), IAppDbContext
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
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

        // 1. EmployeePayment Relationships
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

        // 2. Sale Relationships
        builder.Entity<Sale>()
            .HasOne<ApplicationUser>()
            .WithMany(u => u.Sales)
            .HasForeignKey("EmployeeId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // 3. PurchaseOrder Relationships
        builder.Entity<PurchaseOrder>()
            .HasOne<ApplicationUser>()
            .WithMany(u => u.PurchaseOrders)
            .HasForeignKey("CreatedById")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // 4. Customers
        builder.Entity<Customer>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100);
            e.Property(c => c.Phone).HasMaxLength(30);
            e.Property(c => c.AlternatePhone).HasMaxLength(30);
            e.Property(c => c.NidNumber).HasMaxLength(30);
            e.Property(c => c.Address).HasMaxLength(300);
            e.Property(c => c.IsActive).HasDefaultValue(true);
            e.HasIndex(c => c.Phone);
            e.HasIndex(c => c.NidNumber).HasFilter("[NidNumber] IS NOT NULL");
        });

        // 5. Dealers
        builder.Entity<Dealer>(e =>
        {
            e.Property(d => d.Name).HasMaxLength(150);
            e.Property(d => d.ContactPerson).HasMaxLength(100);
            e.Property(d => d.Phone).HasMaxLength(30);
            e.Property(d => d.Address).HasMaxLength(300);
            e.Property(d => d.IsActive).HasDefaultValue(true);
        });

        // 6. Products (Brand, Barcode, ReorderLevel, DefaultDealer, AllowInstallment)
        builder.Entity<Product>(e =>
        {
            e.Property(p => p.Name).HasMaxLength(150);
            e.Property(p => p.Brand).HasMaxLength(100).HasDefaultValue("Jamuna");
            e.Property(p => p.ModelNo).HasMaxLength(100);
            e.Property(p => p.Category).HasMaxLength(100);
            e.Property(p => p.Barcode).HasMaxLength(100);
            e.Property(p => p.BuyingPrice).HasPrecision(18, 2);
            e.Property(p => p.SellingPrice).HasPrecision(18, 2);
            e.Property(p => p.ReorderLevel).HasDefaultValue(5);
            e.Property(p => p.AllowInstallment).HasDefaultValue(true);
            e.Property(p => p.IsActive).HasDefaultValue(true);

            e.HasIndex(p => p.ModelNo).IsUnique();
            e.HasIndex(p => p.Brand);
            e.HasIndex(p => p.Category);
            e.HasIndex(p => p.Barcode)
                .IsUnique()
                .HasFilter("[Barcode] IS NOT NULL");

            e.HasOne(p => p.DefaultDealer)
                .WithMany()
                .HasForeignKey(p => p.DefaultDealerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // 7. Sales (DownPayment, ServiceCharge, Guarantor fields)
        builder.Entity<Sale>(e =>
        {
            e.Property(s => s.InvoiceNo).HasMaxLength(50);
            e.Property(s => s.TotalAmount).HasPrecision(18, 2);
            e.Property(s => s.DownPayment).HasPrecision(18, 2);
            e.Property(s => s.PaidAmount).HasPrecision(18, 2);
            e.Property(s => s.ServiceCharge).HasPrecision(18, 2);
            e.Property(s => s.ReturnedAmount).HasPrecision(18, 2);
            e.Property(s => s.RefundedAmount).HasPrecision(18, 2);
            e.Property(s => s.DiscountAmount).HasPrecision(18, 2);

            e.Property(s => s.GuarantorName).HasMaxLength(100);
            e.Property(s => s.GuarantorPhone).HasMaxLength(30);
            e.Property(s => s.GuarantorAddress).HasMaxLength(300);
            e.Property(s => s.GuarantorNid).HasMaxLength(30);

            e.Property(s => s.RowVersion).IsRowVersion();
            e.HasIndex(s => s.InvoiceNo).IsUnique();
            e.HasIndex(s => s.CreatedAt);
        });

        // 8. SaleItems (including SerialNumber captured on sale)
        builder.Entity<SaleItem>(e =>
        {
            e.Property(si => si.UnitPrice).HasPrecision(18, 2);
            e.Property(si => si.CostPrice).HasPrecision(18, 2);
            e.Property(si => si.SerialNumber).HasMaxLength(100);
            e.HasOne(x => x.StockItem)
                .WithMany()
                .HasForeignKey(x => x.StockItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 9. PurchaseOrders & Items
        builder.Entity<PurchaseOrder>(e =>
        {
            e.Property(p => p.DealerInvoiceNo).HasMaxLength(50);
            e.Property(p => p.RowVersion).IsRowVersion();
            e.HasIndex(p => new { p.DealerId, p.DealerInvoiceNo }).IsUnique();
        });

        builder.Entity<PurchaseOrderItem>()
            .Property(pi => pi.UnitCost)
            .HasPrecision(18, 2);

        // 10. InstallmentPlans (including CollectionNote & follow-up dates)
        builder.Entity<InstallmentPlan>(e =>
        {
            e.Property(i => i.AmountDue).HasPrecision(18, 2);
            e.Property(i => i.AmountPaid).HasPrecision(18, 2);
            e.Property(i => i.CollectionNote).HasMaxLength(500);
            e.Property(i => i.RowVersion).IsRowVersion();
            e.HasIndex(i => new { i.SaleId, i.InstallmentNumber }).IsUnique();
        });

        // 11. EmployeePayments
        builder.Entity<EmployeePayment>(e =>
        {
            e.Property(ep => ep.BaseSalary).HasPrecision(18, 2);
            e.Property(ep => ep.Bonus).HasPrecision(18, 2);
            e.Property(ep => ep.Deductions).HasPrecision(18, 2);
            e.Property(ep => ep.NetPaidAmount).HasPrecision(18, 2);
            e.Property(ep => ep.Remarks).HasMaxLength(500);
            e.HasIndex(p => new { p.EmployeeId, p.PaymentMonth, p.PaymentYear }).IsUnique();
        });

        // 12. StockItems
        builder.Entity<StockItem>(e =>
        {
            e.Property(s => s.SerialNumber).HasMaxLength(100);
            e.Property(s => s.RowVersion).IsRowVersion();
            e.HasIndex(s => s.SerialNumber)
                .IsUnique()
                .HasFilter("[SerialNumber] IS NOT NULL");
            e.HasIndex(s => new { s.ProductId, s.Location, s.IsFaulty });
        });

        // 13. WarrantyServices
        builder.Entity<WarrantyService>(e =>
        {
            e.Property(w => w.ProductSerialNumber).HasMaxLength(100);
            e.Property(w => w.IssueDescription).HasMaxLength(500);
            e.Property(w => w.RequiredParts).HasMaxLength(300);
        });

        // 14. Audit column lengths
        foreach (var clrType in builder.Model.GetEntityTypes()
                                       .Select(t => t.ClrType)
                                       .Where(t => typeof(BaseEntity).IsAssignableFrom(t))
                                       .ToList())
        {
            builder.Entity(clrType).Property(nameof(BaseEntity.CreatedBy)).HasMaxLength(450);
            builder.Entity(clrType).Property(nameof(BaseEntity.UpdatedBy)).HasMaxLength(450);
        }

        // 15. SaleReturns
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

        // 16. Sale Payments (including MobileBankingProvider & TransactionId)
        builder.Entity<Payment>(e =>
        {
            e.Property(p => p.Amount).HasPrecision(18, 2);
            e.Property(p => p.Method).HasMaxLength(30);
            e.Property(p => p.MobileBankingProvider).HasMaxLength(30);
            e.Property(p => p.TransactionId).HasMaxLength(100);
            e.Property(p => p.ReceiptNo).HasMaxLength(50);
            e.HasIndex(p => p.SaleId);
            e.HasIndex(p => p.Date);
            e.HasOne(p => p.Sale)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.SaleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 17. DealerPayments, Expenses, StockAdjustments, DealerReturns
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

        // 18. AuditLog table
        builder.Entity<AuditLog>(e =>
        {
            e.Property(x => x.EntityName).HasMaxLength(100);
            e.Property(x => x.EntityId).HasMaxLength(100);
            e.Property(x => x.Action).HasMaxLength(20);
            e.Property(x => x.UserId).HasMaxLength(450);
            e.HasIndex(x => x.TimestampUtc);
            e.HasIndex(x => new { x.EntityName, x.EntityId });
        });
    }
}