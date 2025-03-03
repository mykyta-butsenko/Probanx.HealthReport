using Probanx.HealthReport.Models;

namespace Probanx.HealthReport.Interfaces;

public interface IHealthReportGenerator
{
    IEnumerable<ServiceReport> CreateHealthReport(List<HealthDataItem> healthData, int pastDaysCount);
}