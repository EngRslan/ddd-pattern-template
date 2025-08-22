using Engrslan.Application;
using Engrslan.Domain;
using Engrslan.EfCore;
using Engrslan.HttpApi;
//#if (EnableHealthChecks)
using Engrslan.HttpApi.Host.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
//#endif
using Serilog;

// ================================================================================
// Bootstrap Configuration
// ================================================================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Engrslan API Host");
    
    var builder = WebApplication.CreateBuilder(args);
    
    // Configure services
    ConfigureSerilog(builder);
    ConfigureServices(builder);
    //#if (EnableHealthChecks)
    ConfigureHealthChecks(builder);
    //#endif
    //#if(UseAngular)
    ConfigureSpaStaticFiles(builder);
    //#endif
    var app = builder.Build();
    
    // Configure middleware pipeline
    ConfigureMiddleware(app);
    //#if (EnableHealthChecks)
    ConfigureHealthCheckEndpoints(app);
    //#endif
    // Configure API endpoints
    app.UseHttpApi();
    //#if(UseAngular)
    ConfigureSpa(app);
    //#endif
    // ================================================================================
    // Application Startup
    // ================================================================================
    Log.Information("Application started successfully");
    app.Run();
}
catch (Exception ex) when (ex.GetType().Name != "HostAbortedException")
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.Information("Shutting down application");
    Log.CloseAndFlush();
}
return;

void ConfigureSerilog(WebApplicationBuilder webApplicationBuilder)
{
    webApplicationBuilder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("Application", "Engrslan")
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [{RequestId}] {Message:lj}{NewLine}{Exception}",
            fileSizeLimitBytes: 10_485_760, // 10MB
            rollOnFileSizeLimit: true,
            retainedFileCountLimit: 30));
}

void ConfigureServices(WebApplicationBuilder webApplicationBuilder)
{
    // Add core services
    webApplicationBuilder.Services.AddDomain();
    webApplicationBuilder.Services.AddEfCore(webApplicationBuilder.Configuration);
    webApplicationBuilder.Services.AddApplication();
    webApplicationBuilder.Services.AddHttpApi(webApplicationBuilder.Configuration);
    
    //#if (EnableHealthChecks)
    // Configure Health Check Options
    webApplicationBuilder.Services.Configure<MemoryHealthCheckOptions>(options =>
    {
        options.MaximumWorkingSetMB = webApplicationBuilder.Configuration.GetValue<double>("HealthChecks:Memory:MaximumWorkingSetMB", 1024);
        options.CriticalWorkingSetMB = webApplicationBuilder.Configuration.GetValue<double>("HealthChecks:Memory:CriticalWorkingSetMB", 2048);
    });
    //#endif
}
//#if (EnableHealthChecks)
void ConfigureHealthChecks(WebApplicationBuilder webApplicationBuilder)
{
    var connectionString = webApplicationBuilder.Configuration.GetConnectionString("DefaultConnection");
    var healthChecksBuilder = webApplicationBuilder.Services.AddHealthChecks();
    
    if (!string.IsNullOrEmpty(connectionString))
    {
        healthChecksBuilder
            .AddDbContextCheck<ApplicationDataContext>(
                name: "database",
                tags: ["db", "sql", "sqlserver"])
            .AddSqlServer(
                connectionString: connectionString,
                name: "sql-server",
                tags: ["db", "sql", "sqlserver"]);
    }
    
    // Add basic health checks (always included)
    healthChecksBuilder
        .AddCheck<ApplicationHealthCheck>(
            name: "application",
            tags: ["app", "ready"])
        .AddCheck<MemoryHealthCheck>(
            name: "memory",
            tags: ["memory"]);
}
//#endif
//#if(UseAngular)
void ConfigureSpaStaticFiles(WebApplicationBuilder webApplicationBuilder)
{
    webApplicationBuilder.Services.AddSpaStaticFiles(configuration =>
    {
        configuration.RootPath = "ClientApp/dist";
    });
}
//#endif
void ConfigureMiddleware(WebApplication application)
{
    // Exception handling
    if (application.Environment.IsDevelopment())
    {
        application.UseDeveloperExceptionPage();
    }
    
    // Request logging
    ConfigureSerilogRequestLogging(application);
    
    // Security and static files
    application.UseHttpsRedirection();
    application.UseStaticFiles();
    
    //#if(UseAngular)
    if (!application.Environment.IsDevelopment())
    {
        application.UseSpaStaticFiles();
    }
    //#endif
}

void ConfigureSerilogRequestLogging(WebApplication application)
{
    application.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            if (httpContext.Request.Host.HasValue)
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            }
            
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            
            var userAgent = httpContext.Request.Headers.UserAgent.FirstOrDefault();
            if (!string.IsNullOrEmpty(userAgent))
            {
                diagnosticContext.Set("UserAgent", userAgent);
            }
            
            diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        };
    });
}

//#if (EnableHealthChecks)
void ConfigureHealthCheckEndpoints(WebApplication application)
{
    application.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        AllowCachingResponses = false
    });
    
    application.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        AllowCachingResponses = false
    });
    
    application.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false, // Always healthy for liveness
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        AllowCachingResponses = false
    });
}
//#endif
//#if(UseAngular)
void ConfigureSpa(WebApplication application)
{
    
    // Configure SPA
    application.UseSpa(spa =>
    {
        spa.Options.SourcePath = "ClientApp";
        
        if (application.Environment.IsDevelopment())
        {
            // Use proxy to external Angular dev server (faster for development)
            spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
            
            // Alternative: Use the Angular CLI development server
            // spa.UseAngularCliServer(npmScript: "start");
        }
    });
}
//#if(UseAngular)