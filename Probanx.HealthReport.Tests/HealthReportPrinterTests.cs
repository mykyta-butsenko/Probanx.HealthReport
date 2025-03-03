using NSubstitute;
using Probanx.HealthReport.Models;
using Microsoft.Extensions.Logging;

namespace Probanx.HealthReport.Tests;

[TestFixture]
public class HealthReportPrinterTests
{
    private ILogger<HealthReportPrinter> _logger;
    private HealthReportPrinter _healthReportPrinter;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<MockLogger<HealthReportPrinter>>();
        _healthReportPrinter = new HealthReportPrinter(_logger);
    }

    [Test]
    public void PrintHealthReport_ShouldLogUnavailableMessage_WhenReportDataIsUnavailable()
    {
        // Arrange
        var report = new ServiceReport("ServiceA", DateTimeOffset.UtcNow, TimeSpan.Zero, 0, 0, 0);
        var reports = new List<ServiceReport> { report };

        // Act
        _healthReportPrinter.PrintHealthReport(reports);

        // Assert
        _logger.Received(1).LogInformation(
            "Health data for Service name = {serviceName} for Date = {date} is Unavailable",
            "ServiceA",
            $"{report.Date:D}");
    }

    [Test]
    public void PrintHealthReport_ShouldLogCorrectMessage_WhenReportDataIsAvailable()
    {
        // Arrange
        var report = new ServiceReport("ServiceA", DateTimeOffset.UtcNow, TimeSpan.FromHours(1), 50, 30, 20);
        var reports = new List<ServiceReport> { report };

        // Act
        _healthReportPrinter.PrintHealthReport(reports);

        // Assert
        _logger.Received(1).LogInformation(
            "Service name = {serviceName}; Date = {date}; Uptime = {uptime}; UptimePercent = {uptimePercent}; UnhealthyPercent = {unhealthyPercent}; DegradedPercent = {degradedPercent}",
            "ServiceA",
            $"{report.Date:D}",
            "1:00:00",
            "50.00%",
            "30.00%",
            "20.00%");
    }

    [Test]
    public void PrintHealthReport_ShouldLogMultipleMessages_WhenMultipleReportsAreProvided()
    {
        // Arrange
        var report1 = new ServiceReport("ServiceA", DateTimeOffset.UtcNow, TimeSpan.FromHours(1), 50, 30, 20);
        var report2 = new ServiceReport("ServiceB", DateTimeOffset.UtcNow, TimeSpan.Zero, 0, 0, 0);
        var reports = new List<ServiceReport> { report1, report2, };

        // Act
        _healthReportPrinter.PrintHealthReport(reports);

        // Assert
        _logger.Received(1).LogInformation(
            "Service name = {serviceName}; Date = {date}; Uptime = {uptime}; UptimePercent = {uptimePercent}; UnhealthyPercent = {unhealthyPercent}; DegradedPercent = {degradedPercent}",
            "ServiceA",
            $"{report1.Date:D}",
            "1:00:00",
            "50.00%",
            "30.00%",
            "20.00%");

        _logger.Received(1).LogInformation(
            "Health data for Service name = {serviceName} for Date = {date} is Unavailable",
            "ServiceB",
            $"{report2.Date:D}");
    }
}