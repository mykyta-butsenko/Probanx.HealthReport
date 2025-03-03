using Microsoft.Extensions.Logging;
using Probanx.HealthReport.Interfaces;
using Probanx.HealthReport.Models;

namespace Probanx.HealthReport;

public class HealthReportPrinter : IHealthReportPrinter
{
    private readonly ILogger<HealthReportPrinter> _logger;

    public HealthReportPrinter(ILogger<HealthReportPrinter> logger)
    {
        _logger = logger;
    }

    public void PrintHealthReport(List<ServiceReport> reports)
    {
        foreach (var report in reports)
        {
            if (report.Uptime == TimeSpan.Zero &&
                report is { UptimePercent: 0, UnhealthyPercent: 0, DegradedPercent: 0 })
            {
                _logger.LogInformation(
                    "Health data for Service name = {serviceName} for Date = {date} is Unavailable",
                    report.ServiceName,
                    $"{report.Date:D}");
            }
            else
            {
                _logger.LogInformation(
                    "Service name = {serviceName}; Date = {date}; Uptime = {uptime}; UptimePercent = {uptimePercent}; UnhealthyPercent = {unhealthyPercent}; DegradedPercent = {degradedPercent}",
                    report.ServiceName,
                    $"{report.Date:D}",
                    $"{report.Uptime:g}",
                    $"{report.UptimePercent:N}%",
                    $"{report.UnhealthyPercent:N}%",
                    $"{report.DegradedPercent:N}%");
            }
        }
    }
}