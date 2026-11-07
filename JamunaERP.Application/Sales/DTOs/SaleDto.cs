namespace JamunaERP.Application.Sales.DTOs
{
    public class SaleDto
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DownPayment { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal ReturnedAmount { get; set; }
        public decimal RefundedAmount { get; set; }
        public decimal BalanceDue { get; set; }
        public bool IsInstallment { get; set; }
        public int InstallmentCount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string? GuarantorName { get; set; }
        public string? GuarantorPhone { get; set; }
        public string? GuarantorAddress { get; set; }
        public string? GuarantorNid { get; set; }
        public List<SaleItemDto> Items { get; set; } = new();
    }

    public class SaleItemDto
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}