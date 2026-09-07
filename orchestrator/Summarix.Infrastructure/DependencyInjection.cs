using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Summarix.Application.Interfaces;

namespace Summarix.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Textify client

    // Ollama client

        services.AddScoped<IPdfGenerator, PdfGenerator.PdfGenerator>();

        return services;
    }
}
