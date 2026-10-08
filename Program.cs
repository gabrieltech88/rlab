using RLab.Abstractions;
using RLab.Infrastructure;
using RLab.Infrastructure.OnuInterfaces;
using RLab.Infrastructure.Orchestrator;
using RLab.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddScoped<Orchestrator>();
builder.Services.AddScoped<IOnuInterfaceFactory, OnuInterfaceFactory>();

builder.Services.AddScoped<IIxcService, IxcService>();
builder.Services.AddTransient<IHuaweiBlue, HuaweiBlue>();
builder.Services.AddTransient<IHuaweiRed, HuaweiRed>();
builder.Services.AddTransient<INokia, Nokia>();
builder.Services.AddTransient<INbel, Nbel>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapHub<OrchestratorHub>("/hubs/onu");

app.MapPost("/api/orchestrator/start", async (StartOrchestratorRequest request, Orchestrator orchestrator) =>
{
    await orchestrator.RunAsync(request.Model, request.NumberOfOnus);

    return Results.Ok(new
    {
        message = "Fluxo automatizado finalizado."
    });
});

app.Run();

public record StartOrchestratorRequest(string Model, int NumberOfOnus);