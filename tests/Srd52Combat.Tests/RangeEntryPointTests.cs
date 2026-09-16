using Srd52Combat.Attacks;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.AttackFixtures;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// "Combat / Range / p. 15" and "Combat / Ranged Attacks in Close Combat / p. 15", through the
/// entry points only: <c>normal-and-long-range</c> and <c>ranged-in-close-combat</c>.
/// </summary>
public class RangeEntryPointTests
{
    private static RangeVerdict At(int distanceFeet) =>
        Value<RangeVerdict>(EntryPoints.NormalAndLongRange.Resolve(
            new NormalAndLongRangeRequest { Ranges = Longbow, DistanceFeet = distanceFeet }));

    [Fact]
    public void At_normal_range_no_Disadvantage_beyond_it_Disadvantage_at_long_range_still_possible_beyond_it_not()
    {
        // "Beyond" is strict at both numbers: 150 feet is not beyond the normal range, and 600 feet
        // is not beyond the long range.
        var atNormal = At(Longbow.NormalFeet);
        Assert.Equal(RangeBand.WithinNormalRange, atNormal.Band);
        Assert.Equal(RollEffect.None, atNormal.Effect);
        Assert.True(atNormal.CanAttack);

        var justBeyondNormal = At(Longbow.NormalFeet + 1);
        Assert.Equal(RangeBand.BeyondNormalRange, justBeyondNormal.Band);
        Assert.Equal(RollEffect.Disadvantage, justBeyondNormal.Effect);
        Assert.True(justBeyondNormal.CanAttack);

        var atLong = At(Longbow.LongFeet);
        Assert.Equal(RangeBand.BeyondNormalRange, atLong.Band);
        Assert.Equal(RollEffect.Disadvantage, atLong.Effect);
        Assert.True(atLong.CanAttack);

        var beyondLong = At(Longbow.LongFeet + 1);
        Assert.Equal(RangeBand.BeyondLongRange, beyondLong.Band);
        Assert.False(beyondLong.CanAttack);

        Assert.Equal("Combat / Range / p. 15", atNormal.Authority.Citation);
        Assert.Equal(EntryPoints.NormalAndLongRange.Registered.Locator, atNormal.Authority);
        Assert.Equal(Longbow, atNormal.Ranges);
    }

    [Fact]
    public void The_long_range_is_the_larger_number_and_a_smaller_one_is_refused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TwoRanges(150, 150));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TwoRanges(150, 20));
        Assert.Throws<ArgumentException>(() => EntryPoints.NormalAndLongRange.Resolve(
            new NormalAndLongRangeRequest { DistanceFeet = 10 }));
    }

    [Fact]
    public void A_ranged_attack_within_5_feet_of_a_seeing_capable_enemy_has_Disadvantage_citing_page_15()
    {
        var enemies = EnemiesWithinFiveFeet.Of(
            Gm,
            new NearbyEnemy("the blinded ogre", CanSeeYou: false, Incapacitated: false),
            new NearbyEnemy("the goblin", CanSeeYou: true, Incapacitated: false));

        var determination = Value<RollDetermination>(EntryPoints.RangedInCloseCombat.Resolve(
            new RangedInCloseCombatRequest { Enemies = enemies }));

        Assert.Equal(RollEffect.Disadvantage, determination.Effect);
        Assert.Equal("ranged-in-close-combat", determination.EntryId);
        Assert.Equal("Combat / Ranged Attacks in Close Combat / p. 15", determination.Authority.Citation);
        Assert.Contains("the goblin", determination.Because, StringComparison.Ordinal);
    }

    [Fact]
    public void An_enemy_that_cannot_see_the_attacker_or_is_Incapacitated_does_not_give_Disadvantage()
    {
        EnemiesWithinFiveFeet[] harmless =
        [
            EnemiesWithinFiveFeet.None(Gm),
            EnemiesWithinFiveFeet.Of(Gm, new NearbyEnemy("the blinded ogre", CanSeeYou: false, Incapacitated: false)),
            EnemiesWithinFiveFeet.Of(Gm, new NearbyEnemy("the stunned guard", CanSeeYou: true, Incapacitated: true)),
        ];

        foreach (var enemies in harmless)
        {
            var determination = Value<RollDetermination>(EntryPoints.RangedInCloseCombat.Resolve(
                new RangedInCloseCombatRequest { Enemies = enemies }));

            Assert.Equal(RollEffect.None, determination.Effect);
            Assert.Contains(enemies.ToString(), determination.Because, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void The_enemies_within_5_feet_are_stated_and_never_inferred()
    {
        Assert.Throws<ArgumentException>(() => EntryPoints.RangedInCloseCombat.Resolve(
            RangedInCloseCombatRequest.Empty));
    }
}
