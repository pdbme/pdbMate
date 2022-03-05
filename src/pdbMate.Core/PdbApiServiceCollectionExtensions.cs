using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pdbMate.Core.Interfaces;

namespace pdbMate.Core
{
    public static class PdbApiServiceCollectionExtensions
    {
        public static IServiceCollection AddPdbApiService(this IServiceCollection services, IConfiguration config)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (config == null) throw new ArgumentNullException(nameof(config));

            services.AddScoped<IPdbApiService, PdbApiService>();
            services.Configure<PdbApiServiceOptions>(config);

            return services;
        }
    }
}
