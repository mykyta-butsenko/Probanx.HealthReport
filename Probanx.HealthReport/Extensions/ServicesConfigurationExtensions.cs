using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Probanx.HealthReport.Interfaces;

namespace Probanx.HealthReport.Extensions;

public static class ServicesConfigurationExtensions
{
    public static void AddHealthReportServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IHealthReportGenerator, HealthReportGenerator>();

        services.AddLogging(loggerBuilder =>
        {
            loggerBuilder.AddSimpleConsole();
        });
        services.AddSingleton<IHealthReportPrinter, HealthReportPrinter>();
    }
}