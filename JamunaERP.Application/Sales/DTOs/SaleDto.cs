namespace JamunaERP.Application.Sales.DTOs
{
    public class SaleDto
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }      // original sale value
        public decimal PaidAmount { get; set; }       // cash collected
        public decimal ReturnedAmount { get; set; }   // value of returned goods
        public decimal RefundedAmount { get; set; }   // cash given back
        public decimal BalanceDue { get; set; }       // what the customer still owes now
        public bool IsInstallment { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetAmount { get; set; }        // TotalAmount - ReturnedAmount
        public List<SaleItemDto> Items { get; set; } = new();
    }

    public class SaleItemDto
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}