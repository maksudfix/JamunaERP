namespace JamunaERP.Application.Common;

public static class PaymentMethods
{
    public const string Cash = "Cash";
    public const string Card = "Card";
    public const string MobileBanking = "MobileBanking";

    public const string Bkash = "bKash";
    public const string Nagad = "Nagad";
    public const string Rocket = "Rocket";

    public static readonly string[] All = { Cash, Card, MobileBanking };
    public static readonly string[] MobileProviders = { Bkash, Nagad, Rocket };
    public static readonly int[] AllowedInstallmentMonths = { 3, 6, 9, 12 };

    public static bool IsValid(string? method) =>
        !string.IsNullOrWhiteSpace(method) &&
        All.Any(m => string.Equals(m, method.Trim(), StringComparison.OrdinalIgnoreCase));

    public static string Normalize(string? method)
    {
        if (string.IsNullOrWhiteSpace(method)) return Cash;
        var match = All.FirstOrDefault(m =>
            string.Equals(m, method.Trim(), StringComparison.OrdinalIgnoreCase));
        return match ?? method.Trim();
    }

    public static bool IsValidMobileProvider(string? provider) =>
        !string.IsNullOrWhiteSpace(provider) &&
        MobileProviders.Any(p => string.Equals(p, provider.Trim(), StringComparison.OrdinalIgnoreCase));

    public static string? NormalizeMobileProvider(string? provider)
    {
        if (string.IsNullOrWhiteSpace(provider)) return null;
        return MobileProviders.FirstOrDefault(p =>
            string.Equals(p, provider.Trim(), StringComparison.OrdinalIgnoreCase)) ?? provider.Trim();
    }
}