using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.AttackFixtures;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// "Combat / Opportunity Attacks / p. 15", through the entry points only: <c>opportunity-attack</c>
/// and <c>opportunity-attack-avoidance</c>.
/// </summary>
public class OpportunityAttackEntryPointTests
{
    private static readonly LeavingReach Walks = LeavingReach.Walks(Gm);

    private static Resolution<object> Make(
        LeavingReach leaving,
        TargetVisibilityStatement? sight = null,
        ReactionAvailability? reaction = null,
        IncapacitatedStatement? incapacitated = null) =>
        EntryPoints.OpportunityAttack.Resolve(new OpportunityAttackRequest
        {
            Sight = sight ?? Sees,
            Leaving = leaving,
            Reaction = reaction ?? HasReaction,
            Incapacitated = incapacitated ?? NotIncapacitated,
        });

    private static ProvocationVerdict Avoiding(LeavingReach leaving) =>
        Value<ProvocationVerdict>(EntryPoints.OpportunityAttackAvoidance.Resolve(
            new OpportunityAttackAvoidanceRequest { Leaving = leaving }));

    [Fact]
    public void A_seen_creature_leaving_your_reach_offers_one_melee_attack_taken_as_a_Reaction_right_before_it_leaves_citing_page_15()
    {
        var offer = Value<OpportunityAttackOffer>(Make(Walks));

        Assert.True(offer.CanBeMade);
        Assert.True(offer.UsesReaction);
        Assert.Equal(1, offer.Attacks);
        Assert.Equal([MeleeAttackOption.Weapon, MeleeAttackOption.UnarmedStrike], offer.Options.ToArray());
        Assert.Equal("right before it leaves your reach", offer.Timing);
        Assert.Equal("Combat / Opportunity Attacks / p. 15", offer.Authority.Citation);
        Assert.Equal(EntryPoints.OpportunityAttack.Registered.Locator, offer.Authority);
    }

    [Fact]
    public void A_creature_the_attacker_cannot_see_offers_no_Opportunity_Attack()
    {
        var offer = Value<OpportunityAttackOffer>(Make(Walks, sight: TargetVisibilityStatement.HeardNotSeen(Gm)));

        Assert.False(offer.CanBeMade);
        Assert.False(offer.UsesReaction);
        Assert.Equal(0, offer.Attacks);
        Assert.Equal("Combat / Opportunity Attacks / p. 15", offer.Authority.Citation);
    }

    [Fact]
    public void While_avoidance_holds_no_Opportunity_Attack_is_provoked()
    {
        LeavingReach[] avoided =
        [
            LeavingReach.Disengages(Gm),
            new LeavingReach(DepartureMeans.Teleport, DisengageTaken: false, "the creature Teleports away", Gm),
            new LeavingReach(DepartureMeans.MovedWithoutItsOwn, DisengageTaken: false, "an explosion hurls the creature out of reach", Gm),
        ];

        foreach (var leaving in avoided)
        {
            var offer = Value<OpportunityAttackOffer>(Make(leaving));

            Assert.False(offer.CanBeMade);
            Assert.False(Avoiding(leaving).Provokes);
            Assert.Equal(EntryPoints.OpportunityAttackAvoidance.Registered.Locator, offer.Authority);
        }
    }

    [Fact]
    public void While_the_attackers_Reaction_is_spent_it_declines_citing_reactions()
    {
        var spent = ReactionAvailability.AlreadyTaken(Gm);

        var declined = Declined(Make(Walks, reaction: spent));

        Assert.Equal(UnresolvedReason.OutsideCurrentScope, declined.Reason);
        Assert.Equal(EntryPoints.Reactions.Registered.Locator, declined.Locator);
        Assert.Contains(spent.ToString(), declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void While_the_attacker_is_Incapacitated_it_declines_citing_the_condition()
    {
        var declined = Declined(Make(Walks, incapacitated: Incapacitated));

        Assert.Equal(UnresolvedReason.OutsideCurrentScope, declined.Reason);
        Assert.Equal(EntryPoints.IncapacitatedCondition.Registered.Locator, declined.Locator);
        Assert.Contains("Reaction", declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void A_creature_leaving_reach_by_none_of_the_means_the_glossary_lists_declines_citing_the_glossary()
    {
        var elsewhere = new LeavingReach(
            DepartureMeans.ByNoneOfThose,
            DisengageTaken: false,
            "the attacker moves away, so the creature is no longer in its reach",
            Gm);

        var declined = Declined(Make(elsewhere));

        Assert.Equal(UnresolvedReason.OutsideCurrentScope, declined.Reason);
        Assert.Equal(EntryPoints.OpportunityAttacksGlossary.Registered.Locator, declined.Locator);
        Assert.Contains("opportunity-attacks-glossary", declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void Disengage_teleporting_and_being_moved_without_your_own_movement_do_not_provoke_citing_page_15()
    {
        var disengaged = Avoiding(LeavingReach.Disengages(Gm));
        Assert.False(disengaged.Provokes);
        Assert.Contains("Disengage", disengaged.Because, StringComparison.Ordinal);
        Assert.Equal("Combat / Opportunity Attacks / p. 15", disengaged.Authority.Citation);
        Assert.Equal(EntryPoints.OpportunityAttackAvoidance.Registered.Locator, disengaged.Authority);

        var teleported = Avoiding(new LeavingReach(DepartureMeans.Teleport, false, "the creature Teleports away", Gm));
        Assert.False(teleported.Provokes);
        Assert.Contains("Teleport", teleported.Because, StringComparison.Ordinal);
    }

    [Fact]
    public void Both_of_the_corpuss_examples_an_explosion_and_a_fall_do_not_provoke()
    {
        LeavingReach[] examples =
        [
            new(DepartureMeans.MovedWithoutItsOwn, false, "an explosion hurls the creature out of a foe's reach", Gm),
            new(DepartureMeans.MovedWithoutItsOwn, false, "the creature falls past an enemy", Gm),
        ];

        foreach (var leaving in examples)
        {
            var verdict = Avoiding(leaving);

            Assert.False(verdict.Provokes);
            Assert.Contains(leaving.Description, verdict.Because, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Walking_out_of_reach_is_not_avoided_by_this_rule()
    {
        var verdict = Avoiding(Walks);

        Assert.True(verdict.Provokes);
    }

    [Fact]
    public void A_statement_an_Opportunity_Attack_needs_is_refused_when_left_out_never_inferred()
    {
        Assert.Throws<ArgumentException>(() => EntryPoints.OpportunityAttackAvoidance.Resolve(
            OpportunityAttackAvoidanceRequest.Empty));
        Assert.Throws<ArgumentException>(() => EntryPoints.OpportunityAttack.Resolve(
            new OpportunityAttackRequest { Sight = Sees, Leaving = Walks, Reaction = HasReaction }));
        Assert.Throws<ArgumentException>(() => EntryPoints.OpportunityAttack.Resolve(
            new OpportunityAttackRequest { Sight = Sees, Leaving = Walks, Incapacitated = NotIncapacitated }));
    }
}
