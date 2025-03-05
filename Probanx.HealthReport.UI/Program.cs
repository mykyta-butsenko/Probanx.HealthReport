using Microsoft.Extensions.DependencyInjection;
using Probanx.HealthReport.Extensions;
using Probanx.HealthReport.Interfaces;
using Probanx.HealthReport.UI;

const int pastDaysCount = 14;

// Configure services
var serviceCollection = new ServiceCollection();
serviceCollection.AddHealthReportServices();

// Build the service provider
var serviceProvider = serviceCollection.BuildServiceProvider();

// Resolve the dependencies and run the application
var reportGenerator = serviceProvider.GetRequiredService<IHealthReportGenerator>();
var reportPrinter = serviceProvider.GetRequiredService<IHealthReportPrinter>();

var healthData = HealthDataItemGenerator.Generate();
var reports = reportGenerator.CreateHealthReport(healthData, pastDaysCount).ToList();
reportPrinter.PrintHealthReport(reports);
Console.ReadLine();