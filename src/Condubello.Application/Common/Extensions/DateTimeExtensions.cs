namespace Condubello.Application.Common.Extensions;

public static class DateTimeExtensions
{
    /// <summary>PostgreSQL (timestamptz) exige fechas UTC; las fechas sin zona se asumen UTC.</summary>
    public static DateTime ToUtc(this DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
