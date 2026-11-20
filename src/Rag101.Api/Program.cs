using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Cisharpai;
using Cisharpai.Azure;
using Cisharpai.OpenAi;
using Rag101.Api.Bootstrap;
using Rag101.Api.Data;
using Rag101.Api.Settings;
using Rag101.Api.Status;

var builder = WebApplication.CreateBuilder(args);

// Enums travel as text ("Reachable"), not as numbers.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// The Bootstrap connection: the credential store depends on the operating system.
builder.Services.AddSingleton<ICredentialStore>(_ =>
    RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? new MacKeychainCredentialStore()
    : RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new WindowsCredentialStore()
    : new UnsupportedCredentialStore());

builder.Services.AddSingleton(services => new BootstrapConnection(
    services.GetRequiredService<ICredentialStore>(),
    Environment.GetEnvironmentVariable(BootstrapConnection.EnvironmentVariable)));

builder.Services.AddSingleton<MongoDatabaseProvider>();
builder.Services.AddSingleton<SettingsStore>();
builder.Services.AddSingleton<DataReset>();

// Language models, through Cisharpai. The provider is chosen at run time
// from the settings, so we register the factory and the providers we offer.
builder.Services.AddCisharpaiClientFactory()
    .AddOpenAiSupport()
    .AddAzureAiInferenceSupport();
builder.Services.AddSingleton<LanguageModelClientFactory>();
builder.Services.AddSingleton<LanguageModelStatusCheck>();

var app = builder.Build();

// The React frontend is built into wwwroot and served from here:
// one process, one port.
app.UseDefaultFiles();
app.UseStaticFiles();

var api = app.MapGroup("/api");

// ---------------------------------------------------------------------------
// Bootstrap connection. These routes work while the application is
// "not configured": they are how it gets configured.
// ---------------------------------------------------------------------------

api.MapGet("/bootstrap", (BootstrapConnection bootstrap) => new
{
    Configured = bootstrap.IsConfigured,
    bootstrap.Source,
    bootstrap.CanSave,
});

api.MapPost("/bootstrap/test", async (BootstrapRequest request) =>
    await MongoDbStatusCheck.CheckAsync(request.ConnectionString));

api.MapPut("/bootstrap", async (BootstrapRequest request, BootstrapConnection bootstrap) =>
{
    if (!bootstrap.CanSave)
    {
        return Results.Conflict(new
        {
            Error = $"The MongoDB connection is read from the {BootstrapConnection.EnvironmentVariable} " +
                    "environment variable and cannot be saved from the application.",
        });
    }

    // Only a connection string that works is saved.
    var status = await MongoDbStatusCheck.CheckAsync(request.ConnectionString);
    if (status.State != DependencyState.Reachable)
    {
        return Results.BadRequest(new { Error = status.Detail });
    }

    bootstrap.Save(request.ConnectionString);
    return Results.NoContent();
});

// ---------------------------------------------------------------------------
// Everything below needs MongoDB. While the application is "not configured"
// these routes answer 503 instead of failing.
// ---------------------------------------------------------------------------

var configured = api.MapGroup("").AddEndpointFilter(async (context, next) =>
{
    var bootstrap = context.HttpContext.RequestServices.GetRequiredService<BootstrapConnection>();
    return bootstrap.IsConfigured
        ? await next(context)
        : Results.Json(new { Error = "not_configured" }, statusCode: StatusCodes.Status503ServiceUnavailable);
});

configured.MapGet("/settings", async (SettingsStore store) =>
    SettingsResponse.From(await store.LoadAsync()));

configured.MapPut("/settings", async (SettingsUpdate update, SettingsStore store) =>
{
    var stored = await store.LoadAsync();
    await store.SaveAsync(update.ApplyTo(stored));
    return Results.NoContent();
});

// MongoDB and Elasticsearch are checked independently: one failing does not
// hide the other. The language model is not called here.
configured.MapGet("/status", async (BootstrapConnection bootstrap, SettingsStore store) =>
{
    var mongoDb = await MongoDbStatusCheck.CheckAsync(bootstrap.Value!);

    // The Elasticsearch settings are in MongoDB: without MongoDB we cannot
    // know them.
    var elasticsearch = mongoDb.State == DependencyState.Reachable
        ? await ElasticsearchStatusCheck.CheckAsync((await store.LoadAsync()).Elasticsearch)
        : DependencyStatus.Unreachable("The Elasticsearch settings are stored in MongoDB, which is unreachable.");

    return new { MongoDb = mongoDb, Elasticsearch = elasticsearch };
});

configured.MapPost("/status/language-model", async (SettingsStore store, LanguageModelStatusCheck check) =>
    await check.CheckAsync((await store.LoadAsync()).LanguageModel));

configured.MapPost("/reset-data", async (DataReset reset) => await reset.ResetAsync());

// An unknown /api route is an error; any other path is a page of the
// frontend, so the browser gets index.html.
app.MapFallback("/api/{**path}", () => Results.NotFound());
app.MapFallbackToFile("index.html");

app.Run();

record BootstrapRequest(string ConnectionString);
