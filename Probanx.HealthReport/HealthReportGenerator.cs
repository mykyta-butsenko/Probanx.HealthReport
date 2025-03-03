using Microsoft.Extensions.Diagnostics.HealthChecks;
using Probanx.HealthReport.Interfaces;
using Probanx.HealthReport.Models;

namespace Probanx.HealthReport;

public class HealthReportGenerator : IHealthReportGenerator
{
    private readonly IDateTimeProvider _timeProvider;

    public HealthReportGenerator(IDateTimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public IEnumerable<ServiceReport> CreateHealthReport(List<HealthDataItem> healthData, int pastDaysCount)
    {
        var endDate = _timeProvider.OffsetNow;
        var startDate = endDate.Date.AddDays(-pastDaysCount + 1); // Past N days including today

        var groupedData = healthData
            .Where(data => data.Date >= startDate)
            .GroupBy(data => new { data.Service, data.Date.Date })
            .ToDictionary(group => group.Key, group => group.OrderBy(data => data.Date).ToList());

        var serviceNames = healthData
            .Select(data => data.Service)
            .Distinct()
            .ToList();
        var reports = new List<ServiceReport>();

        foreach (var service in serviceNames)
        {
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (!groupedData.TryGetValue(new { Service = service, date.Date }, out var serviceHealthLogsPerDay) || serviceHealthLogsPerDay.Count == 0)
                {
                    reports.Add(new ServiceReport(service, date, TimeSpan.Zero, 0, 0, 0));
                    continue;
                }

                var periodStart = new DateTimeOffset(date.Date, TimeSpan.Zero);
                var periodEnd = periodStart.AddDays(1);
                var totalPeriod = periodEnd - periodStart;

                var healthyTime = TimeSpan.Zero;
                var unhealthyTime = TimeSpan.Zero;
                var degradedTime = TimeSpan.Zero;

                var lastTimestamp = periodStart;
                var lastStatus = serviceHealthLogsPerDay[0].Status;

                foreach (var log in serviceHealthLogsPerDay)
                {
                    var duration = log.Date - lastTimestamp;

                    switch (lastStatus)
                    {
                        case HealthStatus.Healthy:
                            healthyTime += duration;
                            break;
                        case HealthStatus.Unhealthy:
                            unhealthyTime += duration;
                            break;
                        case HealthStatus.Degraded:
                            degradedTime += duration;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"Health status = {lastStatus} is not supported.");
                    }

                    lastTimestamp = log.Date;
                    lastStatus = log.Status;
                }

                // Calculate the remaining time of the day
                var finalDuration = periodEnd - lastTimestamp;
                switch (lastStatus)
                {
                    case HealthStatus.Healthy:
                        healthyTime += finalDuration;
                        break;
                    case HealthStatus.Unhealthy:
                        unhealthyTime += finalDuration;
                        break;
                    case HealthStatus.Degraded:
                        degradedTime += finalDuration;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Health status = {lastStatus} is not supported.");
                }

                var uptimePercent = (healthyTime.TotalSeconds / totalPeriod.TotalSeconds) * 100;
                var unhealthyPercent = (unhealthyTime.TotalSeconds / totalPeriod.TotalSeconds) * 100;
                var degradedPercent = (degradedTime.TotalSeconds / totalPeriod.TotalSeconds) * 100;

                reports.Add(new ServiceReport(service, date, healthyTime, uptimePercent, unhealthyPercent, degradedPercent));
            }
        }

        return reports.OrderBy(report => report.Date);
    }
}