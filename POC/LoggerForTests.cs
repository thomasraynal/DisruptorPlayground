using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorPlayground.POC
{
    public class LoggerForTests : ILogger
    {
        public LoggerForTests()
        {
            Logs = new List<string>();
        }

        public List<string> Logs { get; set; }

        public static ILogger Default()
        {
            return new LoggerForTests();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Logs.Add(state.ToString());
        }
    }
}
