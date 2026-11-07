using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
using JamunaERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.UnitTests.Helpers;

public static class TestDb
{
    // A fresh, isolated InMemory database per test
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    public static async Task<Customer> AddCustomerAsync(AppDbContext db)
    {
        var customer = new Customer { Name = "Test Customer", Phone = "01700000000", Address = "Khulna" };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return customer;
    }

    public static async Task<Product> AddProductAsync(AppDbContext db, decimal price = 1000m, string name = "Test Fan")
    {
        var product = new Product
        {
            Name = name,
            ModelNo = "MODEL-" + Guid.NewGuid().ToString("N")[..6],
            Category = "Fan",
            BuyingPrice = price * 0.8m,
            SellingPrice = price,
            WarrantyMonths = 12
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product;
    }

    public static async Task<StockItem> AddStockAsync(
        AppDbContext db, int productId, int quantity,
        StockLocationType location = StockLocationType.Showroom1, bool isFaulty = false)
    {
        var stock = new StockItem
        {
            ProductId = productId,
            Quantity = quantity,
            Location = location,
            IsFaulty = isFaulty
        };
        db.StockItems.Add(stock);
        await db.SaveChangesAsync();
        return stock;
    }

    // An installment sale with 'count' equal installments of (total - paid) / count, one month apart
    public static async Task<(Sale Sale, List<InstallmentPlan> Plans)> AddInstallmentSaleAsync(
        AppDbContext db, decimal total = 1000m, decimal paid = 100m, int count = 3, DateTime? firstDue = null)
    {
        var customer = await AddCustomerAsync(db);

        var sale = new Sale
        {
            InvoiceNo = "INV-TEST-" + Guid.NewGuid().ToString("N")[..8],
            CustomerId = customer.Id,
            TotalAmount = total,
            PaidAmount = paid,
            IsInstallment = true,
            InstallmentCount = count
        };

        var each = (total - paid) / count;
        var start = (firstDue ?? DateTime.UtcNow.Date.AddDays(10)).Date;

        for (var n = 1; n <= count; n++)
        {
            sale.Installments.Add(new InstallmentPlan
            {
                InstallmentNumber = n,
                AmountDue = each,
                DueDate = start.AddMonths(n - 1),
                Status = InstallmentStatus.Pending
            });
        }

        db.Sales.Add(sale);
        await db.SaveChangesAsync();

        return (sale, sale.Installments.OrderBy(i => i.InstallmentNumber).ToList());
    }
}