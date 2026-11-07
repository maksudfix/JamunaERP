namespace JamunaERP.Application.StockItems.DTOs
{
    public class StockItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty; // e.g. "Showroom1" or "GoDown"
        public int Quantity { get; set; }
        public bool IsFaulty { get; set; }
        public bool IsAvailable { get; set; }
    }
}