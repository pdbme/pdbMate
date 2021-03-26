using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace pdbMate.Core
{
    public static class NzbgetServiceCollectionExtensions
    {
        public static IServiceCollection AddNzbgetService(this IServiceCollection services, IConfiguration config)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (config == null) throw new ArgumentNullException(nameof(config));

            services.AddScoped<INzbgetService, NzbgetService>();
            services.Configure<NzbgetServiceOptions>(config);

            return services;
        }
    }
}
