namespace JamunaERP.Application.Common;

public static class ReceiptNumber
{
    public static string Generate() =>
        $"RCP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}