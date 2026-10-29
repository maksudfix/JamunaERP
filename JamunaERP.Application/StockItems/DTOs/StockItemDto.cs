namespace JamunaERP.Application.StockItems.DTOs
{
    public class StockItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty; // e.g. "Showroom-1" or "Go-down-A"
        public bool IsAvailable { get; set; }
    }
}