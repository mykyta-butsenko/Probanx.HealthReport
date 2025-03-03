using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Probanx.HealthReport.Models;

public record HealthDataItem(string Service, DateTimeOffset Date, HealthStatus Status);