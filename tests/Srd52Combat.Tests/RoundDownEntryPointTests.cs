using RulesKernel.Provenance;
using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// <c>round-down</c>, "Playing the Game / Round Down / p. 5": declined, outside the extent. The
/// engine answers it with nothing but that decline, whatever a caller sends; the one place its
/// arithmetic is done is <c>grid-speed-in-squares</c>, which divides a Speed by 5 and must round
/// down "even if the fraction is one-half or greater".
/// </summary>
public class RoundDownEntryPointTests
{
    private static readonly SourceLocator PageFive = new("srd-5.2.1", "Playing the Game / Round Down / p. 5");

    public static TheoryData<string, RuleRequest> Requests => new()
    {
        { "nothing asserted", RuleRequest.Empty },
        { "an assertion for round-down itself", RuleRequest.Empty.Assert("round-down", "round up") },
        { "an assertion for another entry", RuleRequest.Empty.Assert("grid-play", GridPlayStatement.OnAGrid(Gm)) },
        { "an assertion for no entry at all", RuleRequest.Empty.Assert("not-an-entry", int.MinValue) },
    };

    [Theory]
    [MemberData(nameof(Requests))]
    public void Round_down_declines_outside_scope_citing_page_5_whatever_the_request_asserts(string _, RuleRequest assertions)
    {
        foreach (var resolution in new[]
        {
            EntryPoints.RoundDown.Resolve(new RoundDownRequest(assertions)),
            Registry.Resolve(new RoundDownRequest(assertions)),
            Registry.Resolve("round-down", assertions),
            EntryPoints.RoundDown.Resolve(new RoundDownRequest(assertions)),
        })
        {
            var declined = Declined(resolution);
            Assert.Equal(UnresolvedReason.OutsideCurrentScope, declined.Reason);
            Assert.Equal(PageFive, declined.Locator);
        }
    }

    [Theory]
    [MemberData(nameof(Requests))]
    public void Round_down_never_throws_for_a_request_it_accepts_and_refuses_a_null_one_by_name(string _, RuleRequest assertions)
    {
        Assert.Null(Record.Exception(() => EntryPoints.RoundDown.Resolve(new RoundDownRequest(assertions))));
        Assert.Null(Record.Exception(() => Registry.Resolve("round-down", assertions)));

        Assert.Equal("request", Assert.Throws<ArgumentNullException>(() => EntryPoints.RoundDown.Resolve(null!)).ParamName);
        Assert.Equal("assertions", Assert.Throws<ArgumentNullException>(() => new RoundDownRequest(null!)).ParamName);
    }

    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(4, 0, true)]
    [InlineData(5, 1, false)]
    [InlineData(6, 1, true)]
    [InlineData(7, 1, true)]
    [InlineData(8, 1, true)]
    [InlineData(9, 1, true)]
    [InlineData(10, 2, false)]
    [InlineData(12, 2, true)]
    [InlineData(int.MaxValue, 429_496_729, true)]
    public void A_Speed_in_squares_rounds_down_even_when_the_fraction_is_one_half_or_greater(int feet, int squares, bool roundedDown)
    {
        var speed = Value<SpeedInSquares>(EntryPoints.GridSpeedInSquares.Resolve(
            new GridSpeedInSquaresRequest { SpeedInFeet = feet, Play = GridEntryPointTests.OnAGrid }));

        Assert.Equal(squares, speed.Squares);
        Assert.Equal(roundedDown, speed.RoundedDown);
        Assert.Equal(feet, speed.SpeedInFeet);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void A_negative_Speed_is_refused_before_any_rounding(int feet) =>
        Assert.Equal("speedInFeet", Assert.Throws<ArgumentOutOfRangeException>(() =>
            EntryPoints.GridSpeedInSquares.Resolve(
                new GridSpeedInSquaresRequest { SpeedInFeet = feet, Play = GridEntryPointTests.OnAGrid })).ParamName);
}
