using JamunaERP.Domain.Enums;

namespace JamunaERP.Application.WarrantyServices.DTOs;

public class WarrantyServiceDto
{
    public int Id { get; set; }
    public int SaleItemId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductSerialNumber { get; set; } = string.Empty;
    public DateTime ComplaintDate { get; set; }
    public string IssueDescription { get; set; } = string.Empty;
    public WarrantyStatus Status { get; set; }
    public bool SentBackToDealerForRepair { get; set; }
    public string? RequiredParts { get; set; }
}