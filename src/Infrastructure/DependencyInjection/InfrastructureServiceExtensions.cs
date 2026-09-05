using Application.Abstractions.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Queries;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IUnitOfWork, AppUnitOfWork>();
        services.AddScoped<IMatchQueries, MatchQueries>();

        return services;
    }
}