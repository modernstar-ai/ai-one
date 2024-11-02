namespace agile_chat_api.Utils;

using Microsoft.Extensions.Logging;
using System;

public class ConsoleLogger : ILogger
{
    private readonly string _name;
    private readonly LogLevel _minLogLevel;

    public ConsoleLogger(string name, LogLevel minLogLevel = LogLevel.Information)
    {
        _name = name;
        _minLogLevel = minLogLevel;
    }

    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLogLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var logMessage = formatter(state, exception);
        Console.WriteLine($"{logLevel}: {_name} - {logMessage}");
    }
}