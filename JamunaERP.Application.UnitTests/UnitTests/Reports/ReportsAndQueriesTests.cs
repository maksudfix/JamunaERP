using JamunaERP.Application.Common;
using JamunaERP.Application.Customers.Queries.GetCustomerStatement;
using JamunaERP.Application.Reports.Queries.GetCustomerDues;
using JamunaERP.Application.Reports.Queries.GetDailySalesReport;
using JamunaERP.Application.Reports.Queries.GetDealerPayables;
using JamunaERP.Application.Reports.Queries.GetOverdueInstallments;
using JamunaERP.Application.Reports.Queries.GetProfitReport;
using JamunaERP.Application.Reports.Queries.GetStockSummary;
using JamunaERP.Application.SaleReturns.Commands.CreateSaleReturn;
using JamunaERP.Application.Sales.Commands.CreateSale;
using JamunaERP.Application.Sales.Queries.GetInvoice;
using JamunaERP.Application.Sales.Queries.GetSalesPaged;
using JamunaERP.Application.UnitTests.Helpers;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
using JamunaERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.UnitTests.Reports;

public class ReportsAndQueriesTests
{
    private static async Task<int> SellAsync(AppDbContext db, int qty = 3, decimal paid = 3000m, decimal discount = 0m)
    {
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db, price: 1000m); // BuyingPrice = 800
        await TestDb.AddStockAsync(db, product.Id, 10);

        return await new CreateSaleCommandHandler(db).Handle(
            new CreateSaleCommand(customer.Id, StockLocationType.Showroom1, paid, false, 0, null,
                new List<CreateSaleItemInput> { new(product.Id, qty) }, "Cash", discount),
            CancellationToken.None);
    }

    private static async Task ReturnOneAsync(AppDbContext db, int saleId)
    {
        var itemId = (await db.SaleItems.AsNoTracking().FirstAsync(i => i.SaleId == saleId)).Id;
        await new CreateSaleReturnCommandHandler(db).Handle(
            new CreateSaleReturnCommand(saleId, "Changed mind", StockLocationType.Showroom1,
                new List<SaleReturnLineInput> { new(itemId, 1) }),
            CancellationToken.None);
    }

    [Fact]
    public async Task DailySales_ShowsSalesReturnsAndNetCollected()
    {
        using var db = TestDb.Create();
        var saleId = await SellAsync(db);
        await ReturnOneAsync(db, saleId);

        var report = await new GetDailySalesReportQueryHandler(db).Handle(
            new GetDailySalesReportQuery(ShopTime.Today, ShopTime.Today), CancellationToken.None);

        var row = Assert.Single(report.Rows);
        Assert.Equal(1, row.SalesCount);
        Assert.Equal(3000m, row.SalesAmount);
        Assert.Equal(1000m, row.ReturnedAmount);
        Assert.Equal(2000m, row.NetCollected);   // 3000 in - 1000 refunded
        Assert.Equal(2000m, report.TotalCollected);
    }

    [Fact]
    public async Task Profit_UsesCostSnapshot_NetOfReturns_AndSubtractsExpenses()
    {
        using var db = TestDb.Create();
        var saleId = await SellAsync(db);          // 3 x 1000, cost 800 each
        await ReturnOneAsync(db, saleId);          // net 2 units
        db.Expenses.Add(new Expense
        {
            Title = "Rent",
            Category = "Rent",
            Amount = 500m,
            ExpenseDate = ShopTime.Today.AddHours(12)
        });
        await db.SaveChangesAsync();

        var report = await new GetProfitReportQueryHandler(db).Handle(
            new GetProfitReportQuery(ShopTime.Today, ShopTime.Today), CancellationToken.None);

        Assert.Equal(2000m, report.Revenue);
        Assert.Equal(1600m, report.Cost);
        Assert.Equal(400m, report.GrossProfit);
        Assert.Equal(500m, report.Expenses);
        Assert.Equal(-100m, report.NetProfit);
        Assert.Equal(0, report.ItemsWithoutCost);
    }

    [Fact]
    public async Task CustomerDues_ListsCustomerWithOutstandingBalance()
    {
        using var db = TestDb.Create();
        await TestDb.AddInstallmentSaleAsync(db); // total 1000, paid 100

        var dues = await new GetCustomerDuesQueryHandler(db)
            .Handle(new GetCustomerDuesQuery(), CancellationToken.None);

        var row = Assert.Single(dues);
        Assert.Equal(900m, row.TotalDue);
    }

    [Fact]
    public async Task OverdueInstallments_ListsPastDueUnpaidOnes()
    {
        using var db = TestDb.Create();
        await TestDb.AddInstallmentSaleAsync(db, firstDue: ShopTime.Today.AddDays(-100)); // all 3 are past due

        var overdue = await new GetOverdueInstallmentsQueryHandler(db)
            .Handle(new GetOverdueInstallmentsQuery(), CancellationToken.None);

        Assert.Equal(3, overdue.Count);
        Assert.All(overdue, o => Assert.True(o.DaysOverdue > 0));
    }

    [Fact]
    public async Task DealerPayables_ExcludesSettledDealers_ByDefault()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        var owed = new Dealer { Name = "Owed" };
        var settled = new Dealer { Name = "Settled" };
        var order = new PurchaseOrder { DealerInvoiceNo = "INV-1", Dealer = owed, ArrivalDate = DateTime.UtcNow };
        order.Items.Add(new PurchaseOrderItem { ProductId = product.Id, QuantityArrived = 10, UnitCost = 100m });
        db.PurchaseOrders.Add(order);
        db.Dealers.Add(settled);
        db.DealerPayments.Add(new DealerPayment { Dealer = owed, Amount = 300m });
        await db.SaveChangesAsync();

        var handler = new GetDealerPayablesQueryHandler(db);

        var open = await handler.Handle(new GetDealerPayablesQuery(), CancellationToken.None);
        var row = Assert.Single(open);
        Assert.Equal(700m, row.Outstanding);

        var all = await handler.Handle(new GetDealerPayablesQuery(true), CancellationToken.None);
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task StockSummary_SeparatesGoodAndFaulty_AndFiltersLowStock()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db, price: 1000m); // BuyingPrice = 800
        await TestDb.AddStockAsync(db, product.Id, 5, StockLocationType.Showroom1);
        await TestDb.AddStockAsync(db, product.Id, 2, StockLocationType.GoDown, isFaulty: true);
        var handler = new GetStockSummaryQueryHandler(db);

        var all = await handler.Handle(new GetStockSummaryQuery(), CancellationToken.None);
        var row = Assert.Single(all.Items);
        Assert.Equal(5, row.TotalGood);
        Assert.Equal(2, row.TotalFaulty);
        Assert.Equal(4000m, row.StockValue);

        var low = await handler.Handle(new GetStockSummaryQuery(5), CancellationToken.None);
        Assert.Single(low.Items);

        var none = await handler.Handle(new GetStockSummaryQuery(4), CancellationToken.None);
        Assert.Empty(none.Items);
    }

    [Fact]
    public async Task SalesPaged_PagesAndFilters()
    {
        using var db = TestDb.Create();
        await TestDb.AddInstallmentSaleAsync(db);
        await TestDb.AddInstallmentSaleAsync(db);
        await TestDb.AddInstallmentSaleAsync(db);
        var handler = new GetSalesPagedQueryHandler(db);

        var page1 = await handler.Handle(new GetSalesPagedQuery(Page: 1, PageSize: 2), CancellationToken.None);
        Assert.Equal(2, page1.Items.Count);
        Assert.Equal(3, page1.TotalCount);
        Assert.Equal(2, page1.TotalPages);

        var due = await handler.Handle(new GetSalesPagedQuery(OnlyDue: true), CancellationToken.None);
        Assert.Equal(3, due.TotalCount);

        var none = await handler.Handle(new GetSalesPagedQuery(Search: "no-such-invoice"), CancellationToken.None);
        Assert.Equal(0, none.TotalCount);
    }

    [Fact]
    public async Task CustomerStatement_TotalsAcrossSales()
    {
        using var db = TestDb.Create();
        var (sale, _) = await TestDb.AddInstallmentSaleAsync(db); // total 1000, paid 100

        var statement = await new GetCustomerStatementQueryHandler(db)
            .Handle(new GetCustomerStatementQuery(sale.CustomerId), CancellationToken.None);

        Assert.Single(statement.Sales);
        Assert.Equal(1000m, statement.TotalSales);
        Assert.Equal(100m, statement.TotalPaid);
        Assert.Equal(900m, statement.TotalDue);
    }

    [Fact]
    public async Task Invoice_ShowsDiscountedTotals_LinesAndPayments()
    {
        using var db = TestDb.Create();
        var saleId = await SellAsync(db, qty: 3, paid: 2500m, discount: 500m);

        var invoice = await new GetInvoiceQueryHandler(db)
            .Handle(new GetInvoiceQuery(saleId), CancellationToken.None);

        var line = Assert.Single(invoice.Items);
        Assert.Equal(3, line.Quantity);
        Assert.Equal(3000m, invoice.SubTotal);
        Assert.Equal(500m, invoice.DiscountAmount);
        Assert.Equal(2500m, invoice.TotalAmount);
        Assert.Equal(0m, invoice.BalanceDue);
        Assert.Single(invoice.Payments);
    }
}