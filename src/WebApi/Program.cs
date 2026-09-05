using Infrastructure.DependencyInjection;
using Application.Abstractions.Interfaces;
using WebApi.RealTime;
using WebApi.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSignalR();

builder.Services.AddScoped<IMatchUpdateNotifier,SignalRMatchUpdateNotifier>();

var app = builder.Build();
app.MapHub<MatchHub>("/hubs/match");

app.Run();