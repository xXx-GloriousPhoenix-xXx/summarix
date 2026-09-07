using Microsoft.Extensions.DependencyInjection;
using Summarix.Application.Interfaces;
using Summarix.Application.Services;

namespace Summarix.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISummaryService, SummaryService>();
        return services;
    }
}
