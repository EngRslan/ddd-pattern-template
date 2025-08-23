using System.Reflection;
using System.Runtime.InteropServices;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Engrslan.Features.Samples.Home;


public class HomeEndpoint : EndpointWithoutRequest<HomeResponse>
{
    private static readonly DateTime StartTime = DateTime.UtcNow;
    
    public override void Configure()
    {
        Get("/home");
        Options(x=>x.WithTags("Samples"));
        Summary(s =>
        {
            s.Summary = "Home";
            s.Description = "Returns server information and application details. This is a sample endpoint - DELETE IT IMMEDIATELY in production!";
            s.Responses[200] = "Service is healthy";
        });
        Description(b => b
            .Produces<HomeResponse>(200, "application/json")
            .WithName("Home")
            .WithSummary("Get server and application information")
            .WithDescription("Returns server information and application details. This is a sample endpoint - DELETE IT IMMEDIATELY in production!"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new HomeResponse
        {
            Message = "Welcome to Engrslan API - Domain Driven Design Template",
            Warning = "⚠️ DELETE THIS ENDPOINT IMMEDIATELY! This exposes sensitive server information and should not exist in production!",
            Server = new HomeResponse.ServerInfo
            {
                MachineName = Environment.MachineName,
                OperatingSystem = RuntimeInformation.OSDescription,
                Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                ProcessorCount = Environment.ProcessorCount,
                DotNetVersion = RuntimeInformation.FrameworkDescription,
                ServerTime = DateTime.UtcNow,
                TimeZone = TimeZoneInfo.Local.DisplayName
            },
            Application = new HomeResponse.ApplicationInfo
            {
                Name = Assembly.GetEntryAssembly()?.GetName().Name ?? "Engrslan",
                Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                Uptime = DateTime.UtcNow - StartTime,
                WorkingDirectory = Directory.GetCurrentDirectory()
            }
        };

        await Send.OkAsync(response, ct);
    }
}