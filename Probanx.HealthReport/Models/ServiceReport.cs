namespace Probanx.HealthReport.Models;

public record ServiceReport(string ServiceName, DateTimeOffset Date, TimeSpan Uptime, double UptimePercent, double UnhealthyPercent, double DegradedPercent, double UnavailablePercent);