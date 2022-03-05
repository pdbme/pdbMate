using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System;

public static class ColorConsoleLoggerExtensions
{
    public static ILoggingBuilder AddMateConsoleLogger(
        this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, MateConsoleLoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions
            <MateConsoleLoggerConfiguration, MateConsoleLoggerProvider>(builder.Services);

        return builder;
    }

    public static ILoggingBuilder AddMateConsoleLogger(
        this ILoggingBuilder builder,
        Action<MateConsoleLoggerConfiguration> configure)
    {
        builder.AddMateConsoleLogger();
        builder.Services.Configure(configure);

        return builder;
    }
}
