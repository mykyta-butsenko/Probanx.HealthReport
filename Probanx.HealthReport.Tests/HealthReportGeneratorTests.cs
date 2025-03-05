using Microsoft.Extensions.Diagnostics.HealthChecks;
using Probanx.HealthReport.Interfaces;
using Probanx.HealthReport.Models;
using FluentAssertions;
using NSubstitute;

namespace Probanx.HealthReport.Tests;

[TestFixture]
public class HealthReportGeneratorTests
{
    private IDateTimeProvider _timeProvider;
    private HealthReportGenerator _healthReportGenerator;
    private readonly DateTime _now = new(2025, 03, 03, 18, 00, 00);

    [SetUp]
    public void SetUp()
    {
        _timeProvider = Substitute.For<IDateTimeProvider>();
        _timeProvider.Now.Returns(_now);
        _timeProvider.OffsetNow.Returns(new DateTimeOffset(_now));
        _healthReportGenerator = new HealthReportGenerator(_timeProvider);
    }

    [Test]
    public void CreateHealthReport_ShouldReturnEmptyReport_WhenNoHealthData()
    {
        // Arrange
        var healthData = new List<HealthDataItem>();

        // Act
        var result = _healthReportGenerator.CreateHealthReport(healthData, 7);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void CreateHealthReport_ShouldReturnCorrectReport_ForSingleServiceSingleDay()
    {
        // Arrange
        var healthData = new List<HealthDataItem>
        {
            // Before 2025-03-03 16:00:00, ServiceA didn't have any status (health data for it is Unavailable)
            new("ServiceA", _now.AddHours(-2), HealthStatus.Unhealthy), // At 2025-03-03 16:00:00 ServiceA was Unhealthy
            new("ServiceA", _now.AddHours(-1), HealthStatus.Healthy), // Starting from 2025-03-03 17:00:00 ServiceA was Healthy
        };

        // Act
        var result = _healthReportGenerator.CreateHealthReport(healthData, 1).ToList();

        // Assert
        result.Should().HaveCount(1);
        var report = result.First();
        report.ServiceName.Should().Be("ServiceA");
        report.Date.Date.Should().Be(_now.Date);
        report.UnavailablePercent.Should().Be((double)16 / 24 * 100); // 16 hours out of 24
        report.UnhealthyPercent.Should().Be((double)1 / 24 * 100); // 1 hour out of 24
        report.UptimePercent.Should().Be((double)7 / 24 * 100); // 7 hours out of 24
        report.DegradedPercent.Should().Be(0);
    }

    [Test]
    public void CreateHealthReport_ShouldReturnCorrectReport_ForMultipleServicesMultipleDays()
    {
        // Arrange
        var healthData = new List<HealthDataItem>
        {
            // Before 2025-03-01 16:00:00, ServiceA didn't have any status (health data for it is Unavailable)
            new("ServiceA", _now.AddDays(-2).AddHours(-2), HealthStatus.Healthy),
            new("ServiceA", _now.AddDays(-2).AddHours(-1), HealthStatus.Degraded),
            new("ServiceB", _now.AddDays(-1).AddHours(-2), HealthStatus.Unhealthy),
            new("ServiceB", _now.AddDays(-1).AddHours(-1), HealthStatus.Healthy),
        };

        // Act
        var result = _healthReportGenerator.CreateHealthReport(healthData, 3).ToList();

        // Assert
        result.Should().HaveCount(6); // 2 services * 3 days
        var serviceAReports = result.Where(report => report.ServiceName == "ServiceA").ToList();
        var serviceBReports = result.Where(report => report.ServiceName == "ServiceB").ToList();

        serviceAReports.Should().HaveCount(3);
        serviceBReports.Should().HaveCount(3);

        var firstServiceAReport = serviceAReports[0];
        firstServiceAReport.Date.Date.Should().Be(_now.AddDays(-2).Date);
        firstServiceAReport.UnavailablePercent.Should().Be((double)16 / 24 * 100); // 16 hours out of 24
        firstServiceAReport.UptimePercent.Should().Be((double)1 / 24 * 100); // 1 hour out of 24
        firstServiceAReport.DegradedPercent.Should().Be((double)7 / 24 * 100); // 1 hour out of 24

        var secondServiceAReport = serviceAReports[1];
        secondServiceAReport.Date.Date.Should().Be(_now.AddDays(-1).Date);
        secondServiceAReport.DegradedPercent.Should().Be(100); // Since the last status was Degraded, the whole day is Degraded

        var thirdServiceAReport = serviceAReports[2];
        thirdServiceAReport.Date.Date.Should().Be(_now.Date);
        thirdServiceAReport.DegradedPercent.Should().Be(100); // Since the last status was Degraded, the whole day is Degraded

        var firstServiceBReport = serviceBReports[0];
        firstServiceBReport.Date.Date.Should().Be(_now.AddDays(-2).Date);
        firstServiceBReport.UnavailablePercent.Should().Be(100); // The health data for this day is missing, so the whole day is Unavailable

        var secondServiceBReport = serviceBReports[1];
        secondServiceBReport.Date.Date.Should().Be(_now.AddDays(-1).Date);
        secondServiceBReport.UnavailablePercent.Should().Be((double)16 / 24 * 100); // 16 hours out of 24
        secondServiceBReport.UnhealthyPercent.Should().Be((double)1 / 24 * 100); // 1 hour out of 24
        secondServiceBReport.UptimePercent.Should().Be((double)7 / 24 * 100); // 7 hours out of 24

        var thirdServiceBReport = serviceBReports[2];
        thirdServiceBReport.Date.Date.Should().Be(_now.Date);
        thirdServiceBReport.UptimePercent.Should().Be(100); // Since the last status was Healthy, the whole day is Healthy
    }
}