using AngeloBreuerApi;
using Rokku.Egress.Proxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DataService>();
builder.Services.AddSingleton<GitHubClient>();
builder.Services.Configure<AppConfiguration>(builder.Configuration.GetSection("App"));

builder.Services.AddHttpClient();
builder.Services.AddEgressProxy();

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.MapGet("/", async context =>
{
    var dataService = context
        .RequestServices
        .GetRequiredService<DataService>();

    var data = await dataService
        .FetchAsync(context.RequestAborted)
        .ConfigureAwait(false);

    context.Response.StatusCode = StatusCodes.Status200OK;

    await context.Response
        .WriteAsJsonAsync(data, context.RequestAborted)
        .ConfigureAwait(false);
});

app.Run();
