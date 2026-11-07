namespace JamunaERP.Application.Common;

public static class ShopTime
{
    // Bangladesh has no daylight saving: UTC+6 all year. Change this if the shop is elsewhere.
    public static readonly TimeSpan Offset = TimeSpan.FromHours(6);

    public static DateTime Today => (DateTime.UtcNow + Offset).Date;

    public static DateTime ToLocal(DateTime utc) => utc + Offset;

    // UTC moment when the given local calendar day starts
    public static DateTime StartOfDayUtc(DateTime localDate) => localDate.Date - Offset;
}