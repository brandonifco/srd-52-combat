using Srd52Combat.Attacks;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// What an attack may reach, through the entry points only: <c>reach</c> ("Combat / Reach / p. 15"),
/// <c>melee-within-reach</c> ("Combat / Melee Attacks / p. 15") and <c>single-range</c> ("Combat /
/// Range / p. 15"), the two alternatives to <c>normal-and-long-range</c> that <c>attack-target</c>
/// names.
/// </summary>
public class ReachAndRangeEntryPointTests
{
    [Fact]
    public void A_creature_has_a_five_foot_reach_citing_page_15()
    {
        var reach = Value<CreatureReach>(EntryPoints.Reach.Resolve(new ReachRequest()));

        Assert.Equal(5, reach.Feet);
        Assert.True(reach.IsTheDefault);
        Assert.Null(reach.Greater);
        Assert.Equal("Combat / Reach / p. 15", reach.Authority.Citation);
        Assert.Equal(EntryPoints.Reach.Registered.Locator, reach.Authority);
    }

    [Fact]
    public void A_greater_reach_the_caller_states_is_the_creatures_reach_and_a_smaller_one_is_refused()
    {
        var reach = Value<CreatureReach>(EntryPoints.Reach.Resolve(new ReachRequest
        {
            Greater = new GreaterReach(10, Gm),
        }));

        Assert.Equal(10, reach.Feet);
        Assert.False(reach.IsTheDefault);
        Assert.Equal(Gm, reach.Greater!.StatedBy);

        // "a reach greater than 5 feet": the rule names no smaller one, and the engine invents none.
        Assert.Throws<ArgumentOutOfRangeException>(() => new GreaterReach(5, Gm));
    }

    [Fact]
    public void A_melee_attack_targets_a_creature_within_reach_and_nothing_beyond_it_citing_page_15()
    {
        (int Distance, GreaterReach? Greater, bool Within)[] cases =
        [
            (5, null, true),
            (6, null, false),
            (10, new GreaterReach(10, Gm), true),
            (11, new GreaterReach(10, Gm), false),
        ];

        foreach (var (distance, greater, within) in cases)
        {
            var melee = Value<MeleeTargeting>(EntryPoints.MeleeWithinReach.Resolve(new MeleeWithinReachRequest
            {
                DistanceFeet = distance,
                Greater = greater,
            }));

            Assert.Equal(within, melee.WithinReach);
            Assert.Equal(greater is null ? 5 : 10, melee.Reach.Feet);
            Assert.Equal("Combat / Melee Attacks / p. 15", melee.Authority.Citation);
            Assert.Equal(EntryPoints.MeleeWithinReach.Registered.Locator, melee.Authority);
        }
    }

    [Fact]
    public void A_ranged_attack_with_one_range_cannot_target_beyond_it_citing_page_15()
    {
        (int Distance, bool CanAttack)[] cases = [(0, true), (59, true), (60, true), (61, false)];

        foreach (var (distance, canAttack) in cases)
        {
            var verdict = Value<SingleRangeVerdict>(EntryPoints.SingleRange.Resolve(new SingleRangeRequest
            {
                RangeFeet = 60,
                DistanceFeet = distance,
            }));

            Assert.Equal(canAttack, verdict.CanAttack);
            Assert.Equal(60, verdict.RangeFeet);
            Assert.Equal("Combat / Range / p. 15", verdict.Authority.Citation);
            Assert.Equal(EntryPoints.SingleRange.Registered.Locator, verdict.Authority);
        }
    }

    [Fact]
    public void A_figure_a_reach_or_range_rule_needs_is_refused_when_left_out_never_inferred()
    {
        Assert.Throws<ArgumentException>(() => EntryPoints.MeleeWithinReach.Resolve(new MeleeWithinReachRequest()));
        Assert.Throws<ArgumentException>(() => EntryPoints.SingleRange.Resolve(new SingleRangeRequest { RangeFeet = 60 }));
    }
}
