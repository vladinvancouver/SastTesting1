using Company.DataAccess;
using SecurityTesting1.BackgroundServices;
using SecurityTesting1.Common.Helpers;
using SecurityTesting1.Common.Middleware;
using SecurityTesting1.Common.Objects;
using SecurityTesting1.Common.Services;
using System.Text.Json;

Microsoft.Extensions.Logging.ILogger? _logger = null;

try
{
    var builder = WebApplication.CreateBuilder();

    builder.Logging.ClearProviders();
    builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"))
        .AddConsole();

    ConfigureConfiguration(builder.Configuration);
    ConfigureServices(builder.Configuration, builder.Services);

    var app = builder.Build();
    _logger = app.Services.GetRequiredService<ILogger<Program>>();

    ConfigureLifetime(_logger, app, app.Lifetime);
    ConfigureMiddleware(_logger, app, app.Services, app.Environment);
    ConfigureEndpoints(_logger, app, app.Services);

    app.Run();
}
catch (Exception e)
{
    if (_logger is { })
    {
        _logger.LogError(e, e.Message);
    }
    else
    {
        Console.WriteLine(e.Message);
    }
    throw;
}




void ConfigureConfiguration(ConfigurationManager configuration)
{

}

void ConfigureServices(ConfigurationManager configuration, IServiceCollection services)
{
    //Start background services concurrently.
    services.Configure<HostOptions>(config =>
    {
        config.ServicesStartConcurrently = true;
        config.ServicesStopConcurrently = false;
    });

    services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            builder => builder
                .SetIsOriginAllowed(s => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
            );
    });



    services.AddAuthorization();

    services.AddMvc(options =>
    {
        options.OutputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.HttpNoContentOutputFormatter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new ObjectToInferredTypesConverter());
    });


    // This is required to allow the api controller access to the HttpContext.
    services.AddHttpContextAccessor();

    services.AddScoped(typeof(CallerService));

    //For multipart file upload
    services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(x =>
    {
        x.ValueLengthLimit = int.MaxValue;
        x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart file upload
    });

    ConfigSettings configSettings = GetConfigSettings(configuration);
    services.AddSingleton(configSettings);

    JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };
    jsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    jsonSerializerOptions.Converters.Add(new ObjectToInferredTypesConverter());
    services.AddSingleton(jsonSerializerOptions);

    var repositoryInterfaceTypeToRepositoryTypeMapping = UnitOfWorkHelper.GetRepositoryInterfaceTypeToRepositoryTypeMapping(prefix: "Sql", suffix: "Repository");

    //The two connection strings need to be different so that corresponding connections are pooled separately because of an ADO bug where
    //some settings from transaction connections do not reset.
    //https://github.com/dotnet/SqlClient/issues/96
    string forGeneralUseConnectionStringKey = "ForGeneralUse";
    string forUseWithTransactionsConnectionStringKey = "ForUseWithTransactions";
    string forGeneralUseConnectionString = Microsoft.Extensions.Configuration.ConfigurationExtensions.GetConnectionString(configuration, forGeneralUseConnectionStringKey)!;
    string forUseWithTransactionsConnectionString = Microsoft.Extensions.Configuration.ConfigurationExtensions.GetConnectionString(configuration, forUseWithTransactionsConnectionStringKey)!;

    if (String.IsNullOrWhiteSpace(forGeneralUseConnectionString))
    {
        throw new Exception($"Connection string '{forGeneralUseConnectionStringKey}' is blank.");
    }

    if (String.IsNullOrWhiteSpace(forUseWithTransactionsConnectionString))
    {
        throw new Exception($"Connection string '{forUseWithTransactionsConnectionStringKey}' is blank.");
    }

    if (forGeneralUseConnectionString == forUseWithTransactionsConnectionString)
    {
        throw new Exception($"Connection string '{forGeneralUseConnectionStringKey}' must be different than '{forUseWithTransactionsConnectionStringKey}' so that a different connection pool will be created for each connection string.");
    }

    services.AddSingleton(serviceProvider =>
    {
        IUnitOfWork forGeneralUseUnitOfWork = new SqlUnitOfWork(serviceProvider, forGeneralUseConnectionString, repositoryInterfaceTypeToRepositoryTypeMapping) { WarningThresholdInMilliseconds = configSettings.DatabaseExecutionDurationWarningThresholdInMilliseconds };
        IUnitOfWork forUseWithTransactionsUnitOfWork = new SqlUnitOfWork(serviceProvider, forUseWithTransactionsConnectionString, repositoryInterfaceTypeToRepositoryTypeMapping);
        StorageService storageService = new(forGeneralUseUnitOfWork, forUseWithTransactionsUnitOfWork);
        return storageService;
    });
    //services.AddSingleton(serviceProvider =>
    //{
    //    IUnitOfWork forGeneralUseUnitOfWork = new MemoryUnitOfWork();
    //    IUnitOfWork forUseWithTransactionsUnitOfWork = forGeneralUseUnitOfWork;
    //    StorageService storageService = new(forGeneralUseUnitOfWork, forUseWithTransactionsUnitOfWork);
    //    return storageService;
    //});


    services.AddSingleton<IHostedService, SaveEventsBackgroundService>();

    services.AddSingleton(typeof(EventService));
}

void ConfigureLifetime(Microsoft.Extensions.Logging.ILogger logger, Microsoft.AspNetCore.Builder.IApplicationBuilder app, Microsoft.Extensions.Hosting.IHostApplicationLifetime appLife)
{
    appLife.ApplicationStarted.Register(() => ApplicationStarted(logger));
    appLife.ApplicationStopping.Register(() => ApplicationStopping(logger, app));
    appLife.ApplicationStopped.Register(() => ApplicationStopped(logger));
}

void ConfigureMiddleware(Microsoft.Extensions.Logging.ILogger logger, Microsoft.AspNetCore.Builder.IApplicationBuilder app, System.IServiceProvider services, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
    }

    app.UseHttpsRedirection();
    app.UseCors("CorsPolicy");

    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseStatusCodePages();
    app.UseRouting();
    app.UseAuthorization();

    app.UseCaller();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}");

        endpoints.MapRazorPages();
    });
}

void ConfigureEndpoints(Microsoft.Extensions.Logging.ILogger logger, Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app, System.IServiceProvider services)
{

}

ConfigSettings GetConfigSettings(IConfiguration configuration)
{
    if (!Int32.TryParse(configuration["DatabaseExecutionDurationWarningThresholdInMilliseconds"], out int databaseExecutionDurationWarningThresholdInMilliseconds))
    {
        throw new Exception($"Failed to parse setting 'DatabaseExecutionDurationWarningThresholdInMilliseconds' with value '{configuration["DatabaseExecutionDurationWarningThresholdInMilliseconds"]}'.");
    }

    ConfigSettings configSettings = new ConfigSettings
    {
        DatabaseExecutionDurationWarningThresholdInMilliseconds = databaseExecutionDurationWarningThresholdInMilliseconds
    };

    return configSettings;
}


void ApplicationStarted(Microsoft.Extensions.Logging.ILogger logger)
{
    if (String.IsNullOrWhiteSpace(System.Environment.UserDomainName))
    {
        logger.LogInformation($@"Application starting on process ID {System.Diagnostics.Process.GetCurrentProcess().Id}, name '{System.Diagnostics.Process.GetCurrentProcess().ProcessName}' and running as '{System.Environment.UserName}'.");
    }
    else
    {
        logger.LogInformation($@"Application starting on process ID {System.Diagnostics.Process.GetCurrentProcess().Id}, name '{System.Diagnostics.Process.GetCurrentProcess().ProcessName}' and running as '{System.Environment.UserDomainName}\{System.Environment.UserName}'.");
    }
}

void ApplicationStopping(Microsoft.Extensions.Logging.ILogger logger, IApplicationBuilder app)
{
    logger.LogInformation($"Application stopping on process ID {System.Diagnostics.Process.GetCurrentProcess().Id}.");
}

void ApplicationStopped(Microsoft.Extensions.Logging.ILogger logger)
{
    logger.LogInformation($"Application stopped on process ID {System.Diagnostics.Process.GetCurrentProcess().Id}.");
}

