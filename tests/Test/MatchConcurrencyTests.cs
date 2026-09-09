using Application.Exceptions;
using Domain.Aggregates.MatchAggregate;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace tests.Test;

[Collection("DatabaseTests")] 
public sealed class MatchConcurrencyTests
{
    [Fact]
    public async Task TwoScorers_ClaimSameMatch_OnlyOneSucceeds()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=CricketScoring;Username=postgres;Password=postgres")
            .Options;

        Guid matchId = Guid.Empty;

        try
        {
            // Arrange - create a unique match
            await using (var context = new AppDbContext(options))
            {
                var match = Match.Create(
                    team1Id: Random.Shared.Next(100000, 200000),
                    team2Id: Random.Shared.Next(200001, 300000),
                    maxOvers: 10);

                context.Matches.Add(match);

                await context.SaveChangesAsync();

                matchId = match.Id;
            }

            // Load the same match using two independent DbContexts
            await using var contextA = new AppDbContext(options);
            await using var contextB = new AppDbContext(options);

            var matchA = await contextA.Matches
                .SingleAsync(x => x.Id == matchId);

            var matchB = await contextB.Matches
                .SingleAsync(x => x.Id == matchId);

            // Both requests must start with the same concurrency version
            Assert.Equal(
                matchA.ConcurrencyVersion,
                matchB.ConcurrencyVersion);

            // Two different scorers attempt to claim the same match
            matchA.ClaimScorer(101);
            matchB.ClaimScorer(202);

            // Scorer 101 saves first
            var unitOfWorkA = new AppUnitOfWork(contextA);

            await unitOfWorkA.SaveChangesAsync(
                CancellationToken.None);

            // Scorer 202 has a stale ConcurrencyVersion
            var unitOfWorkB = new AppUnitOfWork(contextB);

            await Assert.ThrowsAsync<ConcurrencyException>(
                () => unitOfWorkB.SaveChangesAsync(
                    CancellationToken.None));

            // Verify only scorer 101 was persisted
            await using var verificationContext =
                new AppDbContext(options);

            var savedMatch = await verificationContext.Matches
                .SingleAsync(x => x.Id == matchId);

            Assert.Equal(101, savedMatch.ActiveScorerId);
        }
        finally
        {
            // Always remove the match created by this test
            if (matchId != Guid.Empty)
            {
                await using var cleanupContext =
                    new AppDbContext(options);

                var match = await cleanupContext.Matches
                    .SingleOrDefaultAsync(x => x.Id == matchId);

                if (match is not null)
                {
                    cleanupContext.Matches.Remove(match);

                    await cleanupContext.SaveChangesAsync();
                }
            }
        }
    }
}