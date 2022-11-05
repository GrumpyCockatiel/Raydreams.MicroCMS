using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Transactions;

namespace Raydreams.MicroCMS
{
    public class NullLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => default!;

        public bool IsEnabled( LogLevel logLevel )
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            return;
        }
    }

    public sealed class NullLoggerProvider : ILoggerProvider
    {

        public NullLoggerProvider()
        {
        }

        public ILogger CreateLogger(string categoryName) => new NullLogger();

        public void Dispose()
        {
            return;
        }
    }
}
