using CertManager.Application;
using CertManager.Domain;
using CertManager.EfCore;
using CertManager.HttpApi;
using CertManager.HttpApi.Host.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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
    Log.Information("Starting CertManager API Host");
    
    var builder = WebApplication.CreateBuilder(args);
    
    // ================================================================================
    // Logging Configuration
    // ================================================================================
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("Application", "CertManager")
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [{RequestId}] {Message:lj}{NewLine}{Exception}",
            fileSizeLimitBytes: 10_485_760, // 10MB
            rollOnFileSizeLimit: true,
            retainedFileCountLimit: 30));
    
    // ================================================================================
    // Service Registration
    // ================================================================================
    
    // Add core services
    builder.Services.AddDomain();
    builder.Services.AddEfCore(builder.Configuration);
    builder.Services.AddApplication();
    builder.Services.AddHttpApi(builder.Configuration);
    
    // Configure Health Check Options
    builder.Services.Configure<MemoryHealthCheckOptions>(options =>
    {
        options.MaximumWorkingSetMB = builder.Configuration.GetValue<double>("HealthChecks:Memory:MaximumWorkingSetMB", 1024);
        options.CriticalWorkingSetMB = builder.Configuration.GetValue<double>("HealthChecks:Memory:CriticalWorkingSetMB", 2048);
    });
    
    // Configure Health Checks
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(connectionString))
    {
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDataContext>(
                name: "database",
                tags: ["db", "sql", "sqlserver"])
            .AddSqlServer(
                connectionString: connectionString,
                name: "sql-server",
                tags: ["db", "sql", "sqlserver"])
            .AddCheck<ApplicationHealthCheck>(
                name: "application",
                tags: ["app", "ready"])
            .AddCheck<MemoryHealthCheck>(
                name: "memory",
                tags: ["memory"]);
    }
    else
    {
        // Add basic health checks if no database connection
        builder.Services.AddHealthChecks()
            .AddCheck<ApplicationHealthCheck>(
                name: "application",
                tags: ["app", "ready"])
            .AddCheck<MemoryHealthCheck>(
                name: "memory",
                tags: ["memory"]);
    }
    
    // Add SPA static files
    builder.Services.AddSpaStaticFiles(configuration =>
    {
        configuration.RootPath = "ClientApp/dist";
    });
    
    // ================================================================================
    // Application Pipeline Configuration
    // ================================================================================
    var app = builder.Build();
    
    // Exception handling
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    
    // Request logging
    app.UseSerilogRequestLogging(options =>
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
    
    // Security and static files
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    
    if (!app.Environment.IsDevelopment())
    {
        app.UseSpaStaticFiles();
    }
    
    // ================================================================================
    // Health Check Endpoints
    // ================================================================================
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        AllowCachingResponses = false
    });
    
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        AllowCachingResponses = false
    });
    
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false, // Always healthy for liveness
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        AllowCachingResponses = false
    });
    
    // ================================================================================
    // API and SPA Configuration
    // ================================================================================
    
    // Configure API endpoints
    app.UseHttpApi();
    
    // Configure SPA
    app.UseSpa(spa =>
    {
        spa.Options.SourcePath = "ClientApp";
        
        if (app.Environment.IsDevelopment())
        {
            // Use proxy to external Angular dev server (faster for development)
            spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
            
            // Alternative: Use the Angular CLI development server
            // spa.UseAngularCliServer(npmScript: "start");
        }
    });
    
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