using JamunaERP.Domain.Common;
using JamunaERP.Domain.Enums;
using System;

namespace JamunaERP.Domain.Entities
{
    public class WarrantyService : BaseEntity
    {
        public int SaleItemId { get; set; }
        public SaleItem SaleItem { get; set; } = null!;

        public string ProductSerialNumber { get; set; } = string.Empty;
        public DateTime ComplaintDate { get; set; } = DateTime.UtcNow;
        public string IssueDescription { get; set; } = string.Empty;

        public WarrantyStatus Status { get; set; } = WarrantyStatus.Active;
        public bool SentBackToDealerForRepair { get; set; }
        public string? RequiredParts { get; set; }
    }
}