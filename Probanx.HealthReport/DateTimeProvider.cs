using Probanx.HealthReport.Interfaces;

namespace Probanx.HealthReport;

// TODO! Use System.TimeProvider introduced in .NET 8 instead: https://learn.microsoft.com/en-us/dotnet/standard/datetime/timeprovider-overview
public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => new(2023, 07, 12, 10, 30, 1);
    public DateTimeOffset OffsetNow => new(Now);
}