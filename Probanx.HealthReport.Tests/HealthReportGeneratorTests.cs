﻿using Microsoft.Extensions.Diagnostics.HealthChecks;
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
            // Now is 2025-03-03 18:00:00.
            // All the timespans and percentages are calculated for 24-hour period (00:00:00-23:59:59) of every day.
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
        report.UptimePercent.Should().Be(37.5);
        report.UnhealthyPercent.Should().Be(62.5);
        report.DegradedPercent.Should().Be(0);
    }

    [Test]
    public void CreateHealthReport_ShouldReturnCorrectReport_ForMultipleServicesMultipleDays()
    {
        // Arrange
        var healthData = new List<HealthDataItem>
        {
            // Now is 2025-03-03 18:00:00.
            // All the timespans and percentages are calculated for 24-hour period (00:00:00-23:59:59) of every day.
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

        var emptyServiceAReports = result.Where(report =>
            report.ServiceName == "ServiceA" && report.Uptime == TimeSpan.Zero &&
            report is { UptimePercent: 0, UnhealthyPercent: 0, DegradedPercent: 0 }).ToList();
        var emptyServiceBReports = result.Where(report =>
            report.ServiceName == "ServiceB" && report.Uptime == TimeSpan.Zero &&
            report is { UptimePercent: 0, UnhealthyPercent: 0, DegradedPercent: 0 }).ToList();

        emptyServiceAReports.Should().HaveCount(2); // For 2025-03-02 and 2025-03-03 (today)
        emptyServiceBReports.Should().HaveCount(2); // For 2025-03-01 and 2025-03-03 (today)

        emptyServiceAReports.Should().Contain(report => report.Date.Date == _now.AddDays(-1).Date || report.Date.Date == _now.Date);
        emptyServiceBReports.Should().Contain(report => report.Date.Date == _now.AddDays(-2).Date || report.Date.Date == _now.Date);
    }
}