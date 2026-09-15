using Domain.Aggregates.MatchAggregate;
using Domain.Enum;

namespace tests.Test;

public sealed class OverTests
{
    private static Match.DeliveryInput Delivery(
        int strikerId = 101,
        int nonStrikerId = 102,
        int bowlerId = 201,
        int batterRuns = 0,
        int totalRuns = 0,
        ExtraType extraType = ExtraType.None,
        Dismissal dismissal = null!,
        int? dismissedPlayerId = null)
    {
        return new Match.DeliveryInput(
            strikerId,
            nonStrikerId,
            bowlerId,
            batterRuns,
            totalRuns,
            extraType,
            dismissal,
            dismissedPlayerId);
    }

    [Fact]
    public void NewOver_ShouldBeEditable()
    {
        var over = Over.Create(1);

        Assert.True(over.IsEditable);
        Assert.False(over.IsCompleted);
    }

    [Fact]
    public void SixLegalDeliveries_ShouldCompleteOver()
    {
        var over = Over.Create(1);

        for (int i = 0; i < 6; i++)
        {
            over.AddDelivery(Delivery());
        }

        Assert.True(over.IsCompleted);
    }

    [Fact]
    public void Wide_ShouldNotCountAsLegalBall()
    {
        var over = Over.Create(1);

        for (int i = 0; i < 5; i++)
        {
            over.AddDelivery(Delivery());
        }

        over.AddDelivery(
            Delivery(
                batterRuns: 0,
                totalRuns: 1,
                extraType: ExtraType.Wide));

        Assert.False(over.IsCompleted);
    }

    [Fact]
    public void NoBall_ShouldNotCountAsLegalBall()
    {
        var over = Over.Create(1);

        for (int i = 0; i < 5; i++)
        {
            over.AddDelivery(Delivery());
        }

        over.AddDelivery(
            Delivery(
                batterRuns: 0,
                totalRuns: 1,
                extraType: ExtraType.NoBall));

        Assert.False(over.IsCompleted);
    }

    [Fact]
    public void UndoLastDelivery_ShouldRemoveDelivery()
    {
        var over = Over.Create(1);

        over.AddDelivery(Delivery());

        Assert.Single(over.Deliveries);

        over.UndoLastDelivery();

        Assert.Empty(over.Deliveries);
    }

    [Fact]
    public void CompletedOver_ShouldNotAcceptAnotherDelivery()
    {
        var over = Over.Create(1);

        for (int i = 0; i < 6; i++)
        {
            over.AddDelivery(Delivery());
        }

        Assert.Throws<InvalidOperationException>(() =>
            over.AddDelivery(Delivery()));
    }

    [Fact]
    public void LockedOver_ShouldNotAcceptDelivery()
    {
        var over = Over.Create(1);

        over.LockEditing();

        Assert.Throws<InvalidOperationException>(() =>
            over.AddDelivery(Delivery()));
    }

    [Fact]
    public void SixDotLegalDeliveries_ShouldBeMaiden()
    {
        var over = Over.Create(1);

        for (int i = 0; i < 6; i++)
        {
            over.AddDelivery(Delivery());
        }

        Assert.True(over.IsMaiden);
    }
}