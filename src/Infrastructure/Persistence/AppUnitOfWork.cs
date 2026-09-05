using Application.Abstractions.Interfaces;
using Application.Exceptions;
using Domain.Aggregates.MatchAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AppUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public AppUnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        foreach (var entry in _context.ChangeTracker.Entries<Match>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.ConcurrencyVersion).CurrentValue = Guid.NewGuid();
            }
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(
                "The match was modified by another user.");
        }
    }
}