using Probanx.HealthReport.Models;

namespace Probanx.HealthReport.Interfaces;

public interface IHealthReportPrinter
{
    void PrintHealthReport(List<ServiceReport> reports);
}