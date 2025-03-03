using Microsoft.Extensions.Logging;

namespace Probanx.HealthReport.Tests;

public abstract class MockLogger<T> : ILogger<T> where T : class
{
    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        Log(logLevel, formatter(state, exception));

    protected abstract void Log(LogLevel logLevel, string message);

    public virtual bool IsEnabled(LogLevel logLevel) => true;

    public abstract IDisposable BeginScope<TState>(TState state) where TState : notnull;
}