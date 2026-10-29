namespace JamunaERP.Application.Reports.DTOs;

public class DailySalesRowDto
{
    public DateTime Date { get; set; }
    public int SalesCount { get; set; }
    public decimal SalesAmount { get; set; }      // net of discount
    public decimal ReturnedAmount { get; set; }
    public decimal NetCollected { get; set; }     // payments in, minus refunds out
}

public class MethodTotalDto
{
    public string Method { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class DailySalesReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public List<DailySalesRowDto> Rows { get; set; } = new();
    public List<MethodTotalDto> ByMethod { get; set; } = new();
    public int TotalSalesCount { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalReturned { get; set; }
    public decimal TotalCollected { get; set; }
}

public class ProductProfitRowDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }          // net of returns
    public decimal Revenue { get; set; }
    public decimal Cost { get; set; }
    public decimal Profit { get; set; }
}

public class ProfitReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal Revenue { get; set; }           // net of discounts and returns
    public decimal Cost { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal Expenses { get; set; }
    public decimal Salaries { get; set; }
    public decimal NetProfit { get; set; }
    public int ItemsWithoutCost { get; set; }      // old sale lines with CostPrice = 0 (profit is overstated)
    public List<ProductProfitRowDto> Products { get; set; } = new();
}

public class CustomerDueDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int SalesWithDue { get; set; }
    public decimal TotalDue { get; set; }
}

public class OverdueInstallmentDto
{
    public int InstallmentId { get; set; }
    public int SaleId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Remaining { get; set; }
    public int DaysOverdue { get; set; }
}

public class DealerPayableDto
{
    public int DealerId { get; set; }
    public string DealerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal TotalPurchased { get; set; }
    public decimal TotalReturned { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Outstanding { get; set; }
}

public class StockSummaryRowDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ModelNo { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Showroom1 { get; set; }             // good units only
    public int Showroom2 { get; set; }
    public int GoDown { get; set; }
    public int TotalGood { get; set; }
    public int TotalFaulty { get; set; }
    public decimal BuyingPrice { get; set; }
    public decimal StockValue { get; set; }        // TotalGood x BuyingPrice
}

public class StockSummaryDto
{
    public List<StockSummaryRowDto> Items { get; set; } = new();
    public int TotalUnits { get; set; }
    public decimal TotalStockValue { get; set; }
}