using Nest;
using SampleApi.Interfaces;
using Serilog.Sinks.Elasticsearch;
using Serilog;
using SampleApi.Models;
using Serilog.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var settings = new ConnectionSettings(new Uri("http://els:9200"))
                    .DefaultIndex("documents");
var client = new ElasticClient(settings);

builder.Services.AddSingleton<IElasticClient>(client);
builder.Services.AddScoped<IDocumentService<Document>, DocumentService>();

configureLogging();
builder.Host.UseSerilog();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();

void configureLogging()
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    if (string.IsNullOrEmpty(environment))
    {
        Console.WriteLine("Environment variable 'ASPNETCORE_ENVIRONMENT' is not set. Using 'Production' as default.");
        environment = "Production";
    }

    Console.WriteLine($"Using environment: {environment}");

    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{environment}.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

    Console.WriteLine("Configuration built successfully.");

    Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .WriteTo.Debug()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
        .Enrich.WithProperty("Environment", environment)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();

    Console.WriteLine("Logging configured successfully.");
}

static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
{
    var elasticUri = configuration["ElasticConfiguration:Uri"];
    if (string.IsNullOrEmpty(elasticUri))
    {
        throw new ArgumentNullException(nameof(elasticUri), "Elastic URI configuration is missing.");
    }

    Console.WriteLine($"Configuring Elasticsearch with URI: {elasticUri}");

    return new ElasticsearchSinkOptions(new Uri(elasticUri))
    {
        AutoRegisterTemplate = true,
        IndexFormat = $"logstash-{environment.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}"
    };
}