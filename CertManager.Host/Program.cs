using CertManager.Application;
using CertManager.Domain;
using CertManager.EfCore;
using CertManager.HttpApi;
using Serilog;

// Create bootstrap logger for startup
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web host");
    
    var builder = WebApplication.CreateBuilder(args);
    
    // Configure Serilog from appsettings.json
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
    
    builder.Services.AddDomain();
    builder.Services.AddEfCore(builder.Configuration);
    builder.Services.AddApplication();
    builder.Services.AddHttpApi(builder.Configuration);
    
    var app = builder.Build();
    
    if(app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            if (httpContext.Request.Host.Value != null)
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            }
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            if (!string.IsNullOrEmpty(httpContext.Request.Headers.UserAgent.FirstOrDefault()))
            {
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault()!);
            }
        };
    });
    
    app.UseHttpsRedirection();
    app.UseHttpApi();
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}