using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pdbMate.Core.Interfaces;

namespace pdbMate.Core
{
    public static class RenameServiceCollectionExtensions
    {
        public static IServiceCollection AddRenameService(this IServiceCollection services, IConfiguration config)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (config == null) throw new ArgumentNullException(nameof(config));

            services.AddScoped<IFileOperatingService, FileOperatingService>();
            services.AddScoped<IRenameService, RenameService>();
            services.AddScoped<IVideoQualityProdiver, VideoQualityProdiver>();
            services.AddScoped<IDuplicateFinder, DuplicateFinder>();
            services.Configure<RenameServiceOptions>(config);
            services.AddScoped<IChangeNamingTemplateService, ChangeNamingTemplateService>();
            services.AddScoped<IRenameWorkflow, RenameWorkflow>();

            return services;
        }
    }
}
