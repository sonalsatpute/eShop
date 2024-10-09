using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace eShop.Observability;

public interface IApplicationTypeChecker
{
    bool IsWebApp();
}

public class ApplicationTypeChecker : IApplicationTypeChecker
{
    private readonly IServiceProvider _serviceProvider;

    public ApplicationTypeChecker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    //Assembly for IWebHostEnvironment: Microsoft.AspNetCore.Hosting.Abstractions 
    public bool IsWebApp() => _serviceProvider.GetService<IWebHostEnvironment>() != null;
}