using eShop.WebApi.Database.Repositories;
using eShop.WebApi.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace eShop.WebApi.Domain;

public static class ServiceConfigurations
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddTransient<IProductService, ProductService>();
        return services;
    }
}