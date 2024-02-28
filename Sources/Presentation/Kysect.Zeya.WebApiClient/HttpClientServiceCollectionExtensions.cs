using Kysect.Zeya.Client.Abstractions.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Kysect.Zeya.WebApiClient;

public static class HttpClientServiceCollectionExtensions
{
    public static IServiceCollection AddZeyaRefit(this IServiceCollection services, Uri baseAddress)
    {
        services
            .AddRefitClient<IValidationPolicyApi>()
            .ConfigureHttpClient(c => c.BaseAddress = baseAddress);

        services
            .AddRefitClient<IValidationPolicyRepositoryApi>()
            .ConfigureHttpClient(c => c.BaseAddress = baseAddress);

        services
            .AddRefitClient<IPolicyValidationApi>()
            .ConfigureHttpClient(c => c.BaseAddress = baseAddress);


        return services;
    }
}