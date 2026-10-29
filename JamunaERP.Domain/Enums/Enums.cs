namespace JamunaERP.Domain.Enums;

public enum StockLocationType
{
    Showroom1 = 1,
    Showroom2 = 2,
    GoDown = 3
}

public enum WarrantyStatus
{
    Active = 1,
    Claimed = 2,
    SentToDealer = 3,
    Repaired = 4,
    Expired = 5
}

public enum InstallmentStatus
{
    Pending = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4   // installment removed because goods were returned
}