using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace pdbMate.Core
{
    public static class SabnzbdServiceCollectionExtensions
    {
        public static IServiceCollection AddSabnzbdService(this IServiceCollection services, IConfiguration config)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (config == null) throw new ArgumentNullException(nameof(config));

            services.AddScoped<ISabnzbdService, SabnzbdService>();
            services.Configure<SabnzbdServiceOptions>(config);

            return services;
        }
    }
}
