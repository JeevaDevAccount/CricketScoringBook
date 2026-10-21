using Application.Abstractions.Interfaces;
using Application.Matches.Commands.MatchCommandHandlers;
using Infrastructure.DependencyInjection;
using Infrastructure.Persistence.Repositories;
using WebApi.Endpoints;
using WebApi.Hubs;
using WebApi.RealTime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173") // React app's local URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
//builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSignalR();

builder.Services.AddScoped<IMatchUpdateNotifier,SignalRMatchUpdateNotifier>();

builder.Services.AddScoped<ClaimScorerHandler>();
builder.Services.AddScoped<ChangeScorerHandler>();
builder.Services.AddScoped<RecordTossHandler>();
builder.Services.AddScoped<CreateMatchHandler>();
builder.Services.AddScoped<StartMatchHandler>();
builder.Services.AddScoped<StartInningsHandler>();
builder.Services.AddScoped<RecordDeliveryHandler>();
builder.Services.AddScoped<UndoDeliveryHandler>();
builder.Services.AddScoped<AddPlayerToPlayingTeamHandler>();
builder.Services.AddScoped<CreatePlayerHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowReactApp");
//app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.MapHub<MatchHub>("/hubs/match");

app.MapMatchesEndpoints();

app.Run();

public partial class Program
{
}