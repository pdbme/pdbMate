using System;
using System.Collections.Concurrent;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[UnsupportedOSPlatform("browser")]
[ProviderAlias("MateConsole")]
public sealed class MateConsoleLoggerProvider : ILoggerProvider
{
    private readonly IDisposable _onChangeToken;
    private MateConsoleLoggerConfiguration _currentConfig;
    private readonly ConcurrentDictionary<string, MateConsoleLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public MateConsoleLoggerProvider(
        IOptionsMonitor<MateConsoleLoggerConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new MateConsoleLogger(name, GetCurrentConfig));

    private MateConsoleLoggerConfiguration GetCurrentConfig() => _currentConfig;

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken.Dispose();
    }
}