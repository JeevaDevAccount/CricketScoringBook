using Domain.Aggregates.MatchAggregate;
using Domain.Enum;

namespace tests.Test;

public sealed class DismissalTests
{
    [Fact]
    public void Create_CaughtWithoutFielder_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            Dismissal.Create(WicketType.Caught, null));
    }

    [Fact]
    public void Create_RunOutWithoutFielder_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            Dismissal.Create(WicketType.RunOut, null));
    }

    [Fact]
    public void Create_StumpedWithoutFielder_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            Dismissal.Create(WicketType.Stumped, null));
    }

    [Fact]
    public void Create_BowledWithFielder_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            Dismissal.Create(WicketType.Bowled, 10));
    }

    [Fact]
    public void Create_None_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            Dismissal.Create(WicketType.None, null));
    }

    [Fact]
    public void Create_CaughtWithFielder_ShouldSucceed()
    {
        var dismissal = Dismissal.Create(
            WicketType.Caught,
            10);

        Assert.Equal(WicketType.Caught, dismissal.WicketType);
        Assert.Equal(10, dismissal.FielderId);
    }
}