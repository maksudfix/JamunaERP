using JamunaERP.Domain.Common;

namespace JamunaERP.Domain.Entities;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public bool IsActive { get; set; } = true;
}