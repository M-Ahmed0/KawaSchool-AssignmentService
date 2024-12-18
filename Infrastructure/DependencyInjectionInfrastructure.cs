using Application.Interfaces.IInfrastructure.IAzureServices;
using Domain.Models.Azure;
using Infrastructure.AzureServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjectionInfrastructure
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(x =>
            new AzureStorageConnectionModel(configuration["AzureStorage:ConnectionString"], "Infrastructure connection string"));

            services.AddTransient<IAzureBlobService, AzureBlobService>();

            return services;
        }
    }
}
