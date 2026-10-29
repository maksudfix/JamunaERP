namespace JamunaERP.Application.Dealers.DTOs;

public class DealerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int PurchaseOrderCount { get; set; }
    public bool IsActive { get; set; }
}