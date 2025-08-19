using CertManager.Domain.Shared.DependencyInjection;

namespace CertManager.Application.Services;

public interface IExampleService
{
    string GetMessage();
}

/// <summary>
/// Example service demonstrating automatic registration with IScopedService marker
/// </summary>
public class ExampleService : IExampleService, IScopedService
{
    public string GetMessage()
    {
        return "This service was automatically registered using IScopedService marker!";
    }
}