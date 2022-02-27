using System;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Extensions.Hosting;
using Serilog.Extensions.Hosting;
using Serilog.Extensions.Logging;
//using Microsoft.Extensions.Logging;

namespace pdbMate
{
    /// <summary>
    /// Extends <see cref="IWebHostBuilder"/> with Serilog configuration methods.
    /// </summary>
    public static class SerilogHostBuilderExtensions
    {
        /// <summary>
        /// Sets Serilog as the logging provider.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="logger">The Serilog logger; if not supplied, the static <see cref="Serilog.Log"/> will be used.</param>
        /// <param name="dispose">When true, dispose <paramref name="logger"/> when the framework disposes the provider. If the
        /// logger is not specified but <paramref name="dispose"/> is true, the <see cref="Serilog.Log.CloseAndFlush()"/> method will be
        /// called on the static <see cref="Serilog.Log"/> class instead.</param>
        /// <param name="providers">A <see cref="LoggerProviderCollection"/> registered in the Serilog pipeline using the
        /// <c>WriteTo.Providers()</c> configuration method, enabling other <see cref="ILoggerProvider"/>s to receive events. By
        /// default, only Serilog sinks will receive events.</param>
        /// <returns>The web host builder.</returns>

        public static IHostBuilder UseSerilog(
            this IHostBuilder builder,
            ILogger logger = null,
            bool dispose = false,
            LoggerProviderCollection providers = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.ConfigureServices(collection =>
            {
                if (providers != null)
                {
                    collection.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(services =>
                    {
                        var factory = new SerilogLoggerFactory(logger, dispose, providers);

                        foreach (var provider in services.GetServices<Microsoft.Extensions.Logging.ILoggerProvider>())
                            factory.AddProvider(provider);

                        return factory;
                    });
                }
                else
                {
                    collection.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(services => new SerilogLoggerFactory(logger, dispose));
                }

                ConfigureServices(collection, logger);
            });

            return builder;
        }

        static void ConfigureServices(IServiceCollection collection, ILogger logger)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            if (logger != null)
            {
                // This won't (and shouldn't) take ownership of the logger. 
                collection.AddSingleton(logger);
            }

            // Registered to provide two services...
            var diagnosticContext = new DiagnosticContext(logger);

            // Consumed by e.g. middleware
            collection.AddSingleton(diagnosticContext);

            // Consumed by user code
            collection.AddSingleton<IDiagnosticContext>(diagnosticContext);
        }
    }
}
