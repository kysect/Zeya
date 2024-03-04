using Kysect.Zeya.Client.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Kysect.Zeya.WebApiClient;

public static class HttpClientServiceCollectionExtensions
{
    public static IServiceCollection AddZeyaRefit(this IServiceCollection services, Uri baseAddress)
    {
        services
            .AddRefitClient<IPolicyService>()
            .ConfigureHttpClient(c => c.BaseAddress = baseAddress);

        services
            .AddRefitClient<IPolicyRepositoryService>()
            .ConfigureHttpClient(c => c.BaseAddress = baseAddress);

        services
            .AddRefitClient<IPolicyValidationService>()
            .ConfigureHttpClient(c => c.BaseAddress = baseAddress);


        return services;
    }
}