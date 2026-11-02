using Microsoft.Extensions.Logging;

namespace Oficina.Infrastructure.UnitTests.Support;

internal sealed record LogEntry(LogLevel Nivel, string Mensagem, Exception? Excecao);

// Fábrica de logs que guarda tudo em memória para os testes conferirem o que foi registrado.
internal sealed class LoggerFactoryCapture : ILoggerFactory
{
    public List<LogEntry> Entradas { get; } = new();

    public ILogger CreateLogger(string categoryName) => new CaptureLogger(Entradas);

    public void AddProvider(ILoggerProvider provider)
    {
    }

    public void Dispose()
    {
    }

    private sealed class CaptureLogger : ILogger
    {
        private readonly List<LogEntry> _entradas;

        public CaptureLogger(List<LogEntry> entradas) => _entradas = entradas;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => _entradas.Add(new LogEntry(logLevel, formatter(state, exception), exception));
    }
}
