namespace JamunaERP.Domain.Enums;

public enum PaymentStatus
{
    Pending = 1,
    Paid = 2,
    Failed = 3,
    PartiallyPaid = 4,
    Refunded = 5,
    Cancelled = 6
}