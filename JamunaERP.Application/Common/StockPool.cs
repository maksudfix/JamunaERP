using JamunaERP.Domain.Entities;

namespace JamunaERP.Application.Common;

public static class StockPool
{
    // Takes 'quantity' units out of the rows, oldest first. The caller checks availability first.
    public static void Deduct(IEnumerable<StockItem> rows, int quantity)
    {
        var remaining = quantity;
        foreach (var row in rows.OrderBy(r => r.Id))
        {
            if (remaining == 0) break;
            var take = Math.Min(row.Quantity, remaining);
            row.Quantity -= take;
            remaining -= take;
        }
    }
}