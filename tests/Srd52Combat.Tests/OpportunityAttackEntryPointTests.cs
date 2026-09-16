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
            new LeavingReach(DepartureMeans.Teleport, DisengageTaken: false, DisengagedOnAnEarlierTurn: false, "the creature Teleports away", Gm),
            new LeavingReach(DepartureMeans.MovedWithoutItsOwn, DisengageTaken: false, DisengagedOnAnEarlierTurn: false, "an explosion hurls the creature out of reach", Gm),
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
            DisengagedOnAnEarlierTurn: false,
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

        var teleported = Avoiding(new LeavingReach(DepartureMeans.Teleport, false, false, "the creature Teleports away", Gm));
        Assert.False(teleported.Provokes);
        Assert.Contains("Teleport", teleported.Because, StringComparison.Ordinal);
    }

    [Fact]
    public void Both_of_the_corpuss_examples_an_explosion_and_a_fall_do_not_provoke()
    {
        LeavingReach[] examples =
        [
            new(DepartureMeans.MovedWithoutItsOwn, false, false, "an explosion hurls the creature out of a foe's reach", Gm),
            new(DepartureMeans.MovedWithoutItsOwn, false, false, "the creature falls past an enemy", Gm),
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
        Assert.Empty(verdict.Decisions);
    }

    [Fact]
    public void Disengage_protects_your_own_movement_on_your_turn_naming_the_engines_decision()
    {
        var verdict = Avoiding(LeavingReach.Disengages(Gm));

        Assert.False(verdict.Provokes);

        // The slice's sentence states no limit; following the glossary's is this engine's own
        // decision, taken by its owner, and every answer that rests on it says so.
        var decision = Assert.Single(verdict.Decisions);
        Assert.Equal("disengage-protection-follows-the-glossary", decision.Id);
        Assert.Equal("opportunity-attack-avoidance", decision.EntryId);
        Assert.Equal("Brandon", decision.DecidedBy);
        Assert.Equal(new DateOnly(2026, 9, 16), decision.DecidedOn);
        Assert.Equal("docs/decisions/0007-brandons-rulings-on-six-open-questions-are-ruleset-version-seven.md", decision.Record);

        // It is not an owner's ruling of rules-factory 0027: this entry has no open question, and
        // nothing about it reaches the generated registry.
        Assert.DoesNotContain(decision.Id, OwnerRulings.All.Select(r => r.Id));
        Assert.DoesNotContain(decision.EntryId, OwnerRulings.All.Select(r => r.EntryId));

        // The two limbs the slice states itself rest on nothing of the engine's.
        Assert.Empty(Avoiding(new LeavingReach(DepartureMeans.Teleport, false, false, "the creature Teleports away", Gm)).Decisions);
        Assert.Empty(Avoiding(new LeavingReach(DepartureMeans.MovedWithoutItsOwn, false, false, "an explosion hurls it clear", Gm)).Decisions);
    }

    [Fact]
    public void A_creature_that_Disengaged_on_an_earlier_turn_provokes_again()
    {
        var later = LeavingReach.DisengagedEarlier(Gm);

        var verdict = Avoiding(later);

        Assert.True(verdict.Provokes);
        Assert.Contains("earlier turn", verdict.Because, StringComparison.Ordinal);
        Assert.Contains("disengage-action", verdict.Because, StringComparison.Ordinal);
        Assert.Equal(
            OwnerDecisions.DisengageCoversYourOwnMovementThisTurn,
            Assert.Single(verdict.Decisions));

        // And the attack is offered, because nothing avoids it any more.
        Assert.True(Value<OpportunityAttackOffer>(Make(later)).CanBeMade);

        // Being hurled out of reach on that later turn is the slice's own second limb, not this
        // creature's lapsed Disengage: it does not provoke, and rests on no decision of the engine's.
        var hurled = Avoiding(LeavingReach.DisengagedEarlier(Gm, DepartureMeans.MovedWithoutItsOwn, "an explosion hurls it clear"));
        Assert.False(hurled.Provokes);
        Assert.Empty(hurled.Decisions);
    }

    [Fact]
    public void An_offer_refused_because_the_creature_Disengaged_names_the_engines_decision()
    {
        var offer = Value<OpportunityAttackOffer>(Make(LeavingReach.Disengages(Gm)));

        Assert.False(offer.CanBeMade);

        // opportunity-attack takes no decision of its own: it carries the avoidance rule's.
        Assert.Equal(OwnerDecisions.DisengageCoversYourOwnMovementThisTurn, Assert.Single(offer.Decisions));

        // An offer that rests on nothing of the engine's names nothing.
        Assert.Empty(Value<OpportunityAttackOffer>(Make(Walks)).Decisions);
        Assert.Empty(Value<OpportunityAttackOffer>(Make(Walks, sight: TargetVisibilityStatement.HeardNotSeen(Gm))).Decisions);
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
