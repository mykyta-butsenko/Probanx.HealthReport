namespace Probanx.HealthReport.Interfaces;

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTimeOffset OffsetNow { get; }
}