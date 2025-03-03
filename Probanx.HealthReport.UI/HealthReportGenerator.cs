using Microsoft.Extensions.Diagnostics.HealthChecks;
using Probanx.HealthReport.Interfaces;
using Probanx.HealthReport.Models;

namespace Probanx.HealthReport.UI;

internal static class HealthReportGenerator
{
    public static List<HealthDataItem> GenerateTestHealthData(int servicesCount, int daysCount, int dataPerDayCount,
        IDateTimeProvider dateTimeProvider)
    {
        var now = dateTimeProvider.OffsetNow;
        var startDate = now.Date.AddDays(-daysCount + 1);
        var services = Enumerable.Range(1, servicesCount).Select(i => $"Service{i}").ToList();
        var statuses = new[] { HealthStatus.Healthy, HealthStatus.Unhealthy, HealthStatus.Degraded };
        var healthData = new List<HealthDataItem>();

        Random random = new();

        for (var dayOffset = 0; dayOffset < daysCount; dayOffset++)
        {
            var date = startDate.AddDays(dayOffset);
            var missingService =
                services[random.Next(services.Count)]; // Randomly select a service to be missing that day
            var availableServices = services.Where(s => s != missingService).ToList();

            foreach (var service in availableServices)
            {
                for (var i = 0; i < dataPerDayCount; i++)
                {
                    var status = statuses[random.Next(statuses.Length)];
                    var timestamp = date.AddHours(random.Next(24)).AddMinutes(random.Next(60));
                    healthData.Add(new HealthDataItem(service, timestamp, status));
                }
            }
        }

        return healthData;
    }

    public static List<HealthDataItem> GenerateTestHealthData()
    {
        return
        [
            new HealthDataItem("Service1", DateTimeOffset.Parse("2023-07-01 05:50:34 +03:00"), HealthStatus.Healthy),
            new HealthDataItem("Service1", DateTimeOffset.Parse("2023-07-02 05:50:34 +03:00"), HealthStatus.Unhealthy),
            new HealthDataItem("Service1", DateTimeOffset.Parse("2023-07-09 05:50:34 +03:00"), HealthStatus.Healthy),
            new HealthDataItem("Service1", DateTimeOffset.Parse("2023-07-10 03:50:34 +03:00"), HealthStatus.Degraded),
            new HealthDataItem("Service1", DateTimeOffset.Parse("2023-07-10 03:55:04 +03:00"), HealthStatus.Healthy),
            new HealthDataItem("Service1", DateTimeOffset.Parse("2023-07-11 03:55:04 +03:00"), HealthStatus.Unhealthy),
            new HealthDataItem("Service1", DateTimeOffset.Parse("2023-07-11 04:15:04 +03:00"), HealthStatus.Healthy)
        ];
    }
}