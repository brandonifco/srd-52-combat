using Srd52Combat.Attacks;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.AttackFixtures;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// The "Unseen Attackers and Targets" sidebar, "Combat / p. 14", through the entry points only:
/// <c>unseen-attacker-advantage</c>, <c>unseen-target-disadvantage</c>,
/// <c>wrong-location-misses</c> and <c>hidden-attacker-revealed</c>.
/// </summary>
public class UnseenAttackersAndTargetsEntryPointTests
{
    [Fact]
    public void A_creature_that_cannot_see_the_attacker_gives_the_attack_roll_Advantage_citing_page_14()
    {
        var unseen = AttackerSeenStatement.Unseen(Gm);

        var determination = Value<RollDetermination>(EntryPoints.UnseenAttackerAdvantage.Resolve(
            new UnseenAttackerAdvantageRequest { Seen = unseen }));

        Assert.Equal(RollEffect.Advantage, determination.Effect);
        Assert.Equal("unseen-attacker-advantage", determination.EntryId);
        Assert.Equal("Combat / Unseen Attackers and Targets / p. 14", determination.Authority.Citation);
        Assert.Equal(EntryPoints.UnseenAttackerAdvantage.Registered.Locator, determination.Authority);
        Assert.Contains(unseen.ToString(), determination.Because, StringComparison.Ordinal);
    }

    [Fact]
    public void A_creature_that_can_see_the_attacker_gives_the_roll_nothing()
    {
        var determination = Value<RollDetermination>(EntryPoints.UnseenAttackerAdvantage.Resolve(
            new UnseenAttackerAdvantageRequest { Seen = AttackerSeenStatement.Seen(Gm) }));

        Assert.Equal(RollEffect.None, determination.Effect);
    }

    [Fact]
    public void A_target_the_attacker_cannot_see_gives_the_roll_Disadvantage_whether_heard_or_guessed_citing_page_14()
    {
        // The sentence's own two cases: guessing the target's location, and a creature you can hear
        // but not see. Both are a target you can't see, and both take Disadvantage.
        TargetVisibilityStatement[] cannotSee =
        [
            TargetVisibilityStatement.LocationGuessed(Gm),
            TargetVisibilityStatement.HeardNotSeen(Gm),
        ];

        foreach (var visibility in cannotSee)
        {
            var determination = Value<RollDetermination>(EntryPoints.UnseenTargetDisadvantage.Resolve(
                new UnseenTargetDisadvantageRequest { Visibility = visibility }));

            Assert.Equal(RollEffect.Disadvantage, determination.Effect);
            Assert.Equal("unseen-target-disadvantage", determination.EntryId);
            Assert.Equal("Combat / Unseen Attackers and Targets / p. 14", determination.Authority.Citation);
            Assert.Contains(visibility.ToString(), determination.Because, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void A_target_the_attacker_can_see_gives_the_roll_nothing()
    {
        var determination = Value<RollDetermination>(EntryPoints.UnseenTargetDisadvantage.Resolve(
            new UnseenTargetDisadvantageRequest { Visibility = Sees }));

        Assert.Equal(RollEffect.None, determination.Effect);
    }

    [Fact]
    public void A_target_that_is_not_in_the_targeted_location_is_missed_citing_page_14()
    {
        var location = new TargetLocationStatement("the far end of the corridor", TargetIsThere: false, Gm);

        var attack = Value<LocationAttack>(EntryPoints.WrongLocationMisses.Resolve(
            new WrongLocationMissesRequest { Location = location }));

        Assert.True(attack.Misses);
        Assert.Equal("the far end of the corridor", attack.TargetedLocation);
        Assert.Equal("Combat / Unseen Attackers and Targets / p. 14", attack.Authority.Citation);
        Assert.Equal(EntryPoints.WrongLocationMisses.Registered.Locator, attack.Authority);
    }

    [Fact]
    public void A_target_that_is_in_the_targeted_location_is_not_missed_by_this_rule()
    {
        var attack = Value<LocationAttack>(EntryPoints.WrongLocationMisses.Resolve(new WrongLocationMissesRequest
        {
            Location = new TargetLocationStatement("the far end of the corridor", TargetIsThere: true, Gm),
        }));

        Assert.False(attack.Misses);
    }

    [Fact]
    public void A_hidden_attacker_gives_away_its_location_whether_the_attack_hits_or_misses_citing_page_14()
    {
        AttackRollOutcome[] outcomes = [AttackRollOutcome.Hits(Gm), AttackRollOutcome.Misses(Gm)];

        foreach (var outcome in outcomes)
        {
            var revealed = Value<HiddenAttacker>(EntryPoints.HiddenAttackerRevealed.Resolve(
                new HiddenAttackerRevealedRequest { Hidden = HiddenStatement.IsHidden(Gm), Outcome = outcome }));

            Assert.True(revealed.LocationGivenAway);
            Assert.Equal("Combat / Unseen Attackers and Targets / p. 14", revealed.Authority.Citation);
            Assert.Contains(outcome.ToString(), revealed.Because, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void An_attacker_that_is_not_hidden_gives_nothing_away()
    {
        var revealed = Value<HiddenAttacker>(EntryPoints.HiddenAttackerRevealed.Resolve(
            new HiddenAttackerRevealedRequest
            {
                Hidden = HiddenStatement.NotHidden(Gm),
                Outcome = AttackRollOutcome.Hits(Gm),
            }));

        Assert.False(revealed.LocationGivenAway);
    }

    [Fact]
    public void A_statement_the_sidebar_needs_is_refused_when_left_out_never_inferred()
    {
        Assert.Throws<ArgumentException>(() => EntryPoints.UnseenAttackerAdvantage.Resolve(
            UnseenAttackerAdvantageRequest.Empty));
        Assert.Throws<ArgumentException>(() => EntryPoints.UnseenTargetDisadvantage.Resolve(
            UnseenTargetDisadvantageRequest.Empty));
        Assert.Throws<ArgumentException>(() => EntryPoints.WrongLocationMisses.Resolve(
            WrongLocationMissesRequest.Empty));
        Assert.Throws<ArgumentException>(() => EntryPoints.HiddenAttackerRevealed.Resolve(
            new HiddenAttackerRevealedRequest { Hidden = HiddenStatement.IsHidden(Gm) }));
    }
}
