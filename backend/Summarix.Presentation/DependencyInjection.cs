using Microsoft.AspNetCore.Http.Features;
using Summarix.Presentation.Configurations;

namespace Summarix.Presentation;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPresentation(IConfiguration configuration, IWebHostBuilder webHostBuilder)
        {
            services.ConfigureUploadFileSize(configuration, webHostBuilder);
            return services;
        }

        private IServiceCollection ConfigureUploadFileSize(IConfiguration configuration, IWebHostBuilder webHostBuilder)
        {
            var fileUploadSettings = configuration
                .GetSection(FileUploadSettings.SectionName)
                .Get<FileUploadSettings>() ?? new FileUploadSettings();

            services.Configure<FileUploadSettings>(
                configuration.GetSection(FileUploadSettings.SectionName)
            );

            webHostBuilder.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = fileUploadSettings.MaxFileSizeInBytes;
            });

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = fileUploadSettings.MaxFileSizeInBytes;
            });

            return services;
        }
    }
}
