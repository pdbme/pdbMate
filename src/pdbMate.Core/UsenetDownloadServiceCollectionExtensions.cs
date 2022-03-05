using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pdbMate.Core.Interfaces;

namespace pdbMate.Core
{
    public static class UsenetDownloadServiceCollectionExtensions
    {
        public static IServiceCollection AddUsenetDownloadService(this IServiceCollection services, IConfiguration config)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (config == null) throw new ArgumentNullException(nameof(config));

            services.AddScoped<IUsenetDownloadService, UsenetDownloadService>();
            services.AddScoped<IVideoMatching, VideoMatching>();
            services.Configure<UsenetDownloadServiceOptions>(config);

            return services;
        }
    }
}
