using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Summarix.Application.Interfaces;
using Summarix.Infrastructure.Ollama;
using Summarix.Infrastructure.PdfGenerator;
using Summarix.Infrastructure.Textify;

namespace Summarix.Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            services.AddTextify(configuration);
            services.AddOllama(configuration);
            services.AddPdfGeneratior(configuration);

            return services;
        }

        public IServiceCollection AddTextify(IConfiguration configuration)
        {
            services.Configure<TextifySettings>(configuration.GetSection(TextifySettings.SectionName));

            var textifySettings = configuration
                .GetSection(TextifySettings.SectionName)
                .Get<TextifySettings>() ?? new TextifySettings();

            services.AddHttpClient<ITextifyClient, TextifyClient>(client =>
            {
                client.BaseAddress = new Uri(textifySettings.BaseUrl);
                client.Timeout = TimeSpan.FromMinutes(textifySettings.TimeoutMinutes);
            });

            return services;
        }

        public IServiceCollection AddOllama(IConfiguration configuration)
        {
            services.Configure<OllamaSettings>(configuration.GetSection(OllamaSettings.SectionName));

            var ollamaSettings = configuration
                .GetSection(OllamaSettings.SectionName)
                .Get<OllamaSettings>() ?? new OllamaSettings();

            services.AddHttpClient<IOllamaClient, OllamaClient>(client =>
            {
                client.BaseAddress = new Uri(ollamaSettings.BaseUrl);
                client.Timeout = TimeSpan.FromMinutes(ollamaSettings.TimeoutMinutes);
            });

            return services;
        }

public IServiceCollection AddPdfGeneratior(IConfiguration configuration)
        {
            services.Configure<PdfSettings>(configuration.GetSection(PdfSettings.SectionName));
            services.AddScoped<IPdfGenerator, PdfGenerator.PdfGenerator>();
            return services;
        }
    }
}
