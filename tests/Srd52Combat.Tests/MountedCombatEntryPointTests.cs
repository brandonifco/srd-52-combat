using RulesKernel.Resolution;
using Srd52Combat.Mounts;
using Srd52Combat.Movement;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// Mounted combat, "Combat / Mounted Combat / p. 15" through "Combat / Falling Off / p. 16", through
/// the entry points only: <c>mount-eligibility</c> and its gap <c>appropriate-anatomy</c>,
/// <c>mounting-cost</c>, <c>mount-control-requires-training</c>, <c>controlled-mount-turn</c>,
/// <c>independent-mount</c> and <c>falling-off</c>.
/// </summary>
public class MountedCombatEntryPointTests
{
    private static readonly MountStatement Ridden = MountStatement.Ridden("the horse", Gm);

    private static readonly MountStatement Serves = MountStatement.Serves("the horse", Gm);

    private static readonly MountStatement NotAMount = MountStatement.NotAMount("the goblin", Gm);

    private static readonly MountCreatureStatement Horse = MountCreatureStatement.DomesticatedHorse("the horse", Gm);

    [Fact]
    public void An_unwilling_creature_and_one_not_at_least_one_size_larger_cannot_serve_as_a_mount_citing_page_15()
    {
        var unwilling = Value<MountEligibility>(EntryPoints.MountEligibility.Resolve(new MountEligibilityRequest
        {
            Rider = CreatureSize.Medium,
            Candidate = "the warhorse",
            CandidateSize = CreatureSize.Large,
            Willing = WillingStatement.IsNot(Gm),
            Anatomy = AnatomyStatement.NotStated("the warhorse", Gm),
        }));

        Assert.False(unwilling.CanServeAsAMount);
        Assert.Contains("willing", unwilling.Because, StringComparison.Ordinal);
        Assert.Equal("Combat / Mounted Combat / p. 15", unwilling.Authority.Citation);
        Assert.Equal(EntryPoints.MountEligibility.Registered.Locator, unwilling.Authority);

        foreach (var size in new[] { CreatureSize.Small, CreatureSize.Medium })
        {
            var tooSmall = Value<MountEligibility>(EntryPoints.MountEligibility.Resolve(new MountEligibilityRequest
            {
                Rider = CreatureSize.Medium,
                Candidate = "the mastiff",
                CandidateSize = size,
                Willing = WillingStatement.Is(Gm),
                Anatomy = AnatomyStatement.NotStated("the mastiff", Gm),
            }));

            Assert.False(tooSmall.CanServeAsAMount);
            Assert.Contains("one size larger", tooSmall.Because, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void A_willing_creature_a_size_larger_turns_on_the_anatomy_and_declines_citing_appropriate_anatomy()
    {
        var declined = Declined(EntryPoints.MountEligibility.Resolve(new MountEligibilityRequest
        {
            Rider = CreatureSize.Medium,
            Candidate = "the giant spider",
            CandidateSize = CreatureSize.Large,
            Willing = WillingStatement.Is(Gm),
            Anatomy = AnatomyStatement.NotStated("the giant spider", Gm),
        }));

        Assert.Equal(UnresolvedReason.RequiresInterpretation, declined.Reason);
        Assert.Equal(EntryPoints.AppropriateAnatomy.Registered.Locator, declined.Locator);
        Assert.Contains("'mount-eligibility'", declined.Attempted, StringComparison.Ordinal);
        Assert.Contains("appropriate-anatomy", declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void A_willing_larger_creature_the_GM_calls_appropriately_shaped_can_serve_as_a_mount_naming_the_ruling()
    {
        var eligible = Value<MountEligibility>(EntryPoints.MountEligibility.Resolve(new MountEligibilityRequest
        {
            Rider = CreatureSize.Medium,
            Candidate = "the giant spider",
            CandidateSize = CreatureSize.Large,
            Willing = WillingStatement.Is(Gm),
            Anatomy = AnatomyStatement.Appropriate("the giant spider", Gm),
        }));

        Assert.True(eligible.CanServeAsAMount);

        // The anatomy limb is what decided it, so the answer carries appropriate-anatomy's ruling.
        Assert.Equal(OwnerRulings.TheGmDecidesTheAnatomy, Assert.Single(eligible.Rulings));

        // And the GM's refusal is an answer too, on the same ruling.
        var refused = Value<MountEligibility>(EntryPoints.MountEligibility.Resolve(new MountEligibilityRequest
        {
            Rider = CreatureSize.Medium,
            Candidate = "the giant spider",
            CandidateSize = CreatureSize.Large,
            Willing = WillingStatement.Is(Gm),
            Anatomy = AnatomyStatement.NotAppropriate("the giant spider", Gm),
        }));

        Assert.False(refused.CanServeAsAMount);
        Assert.Equal(OwnerRulings.TheGmDecidesTheAnatomy, Assert.Single(refused.Rulings));

        // A limb the rule measures itself relies on no ruling at all.
        var unwilling = Value<MountEligibility>(EntryPoints.MountEligibility.Resolve(new MountEligibilityRequest
        {
            Rider = CreatureSize.Medium,
            Candidate = "the giant spider",
            CandidateSize = CreatureSize.Large,
            Willing = WillingStatement.IsNot(Gm),
            Anatomy = AnatomyStatement.Appropriate("the giant spider", Gm),
        }));

        Assert.Empty(unwilling.Rulings);
    }

    [Fact]
    public void A_GM_statement_that_the_anatomy_is_appropriate_answers_naming_the_owners_ruling()
    {
        var ruling = Value<AnatomyRuling>(EntryPoints.AppropriateAnatomy.Resolve(new AppropriateAnatomyRequest
        {
            Anatomy = AnatomyStatement.Appropriate("the giant spider", Gm),
        }));

        Assert.True(ruling.Appropriate);
        Assert.Equal("Combat / Mounted Combat / p. 15", ruling.Authority.Citation);
        Assert.Equal(EntryPoints.AppropriateAnatomy.Registered.Locator, ruling.Authority);

        // Brandon's, not the corpus's: the answer says so, and says where to read it.
        var owners = Assert.Single(ruling.Rulings);
        Assert.Equal("appropriate-anatomy/gm-decides", owners.Id);
        Assert.Equal("appropriate-anatomy", owners.EntryId);
        Assert.Equal("Brandon", owners.RuledBy);
        Assert.Equal(new DateOnly(2026, 9, 16), owners.RuledOn);
        Assert.Equal("docs/decisions/0007-brandons-rulings-on-six-open-questions-are-ruleset-version-seven.md", owners.Record);
        Assert.Contains("names nobody who decides", owners.Span, StringComparison.Ordinal);

        // The GM's refusal is an answer too, and rests on the same ruling.
        var notAppropriate = Value<AnatomyRuling>(EntryPoints.AppropriateAnatomy.Resolve(new AppropriateAnatomyRequest
        {
            Anatomy = AnatomyStatement.NotAppropriate("the giant spider", Gm),
        }));

        Assert.False(notAppropriate.Appropriate);
        Assert.Equal(owners, Assert.Single(notAppropriate.Rulings));
    }

    [Fact]
    public void With_no_GM_statement_the_engine_never_assumes_an_anatomy_and_declines()
    {
        var declined = Declined(EntryPoints.AppropriateAnatomy.Resolve(new AppropriateAnatomyRequest
        {
            Anatomy = AnatomyStatement.NotStated("the giant spider", Gm),
        }));

        Assert.Equal(UnresolvedReason.RequiresInterpretation, declined.Reason);
        Assert.Equal(EntryPoints.AppropriateAnatomy.Registered.Locator, declined.Locator);
        Assert.Contains("no measure", declined.Attempted, StringComparison.Ordinal);
        Assert.Contains("has determined nothing", declined.Attempted, StringComparison.Ordinal);

        // A decline relies on no ruling and names none (rules-factory 0027 § 4).
        Assert.DoesNotContain("gm-decides", declined.Attempted, StringComparison.Ordinal);

        // And the statement is demanded, never inferred in either direction.
        var missing = Assert.Throws<ArgumentException>(() =>
            EntryPoints.AppropriateAnatomy.Resolve(AppropriateAnatomyRequest.Empty));
        Assert.Equal("Anatomy", missing.ParamName);
    }

    [Fact]
    public void Mounting_or_dismounting_costs_half_the_Speed_rounded_down_citing_page_15()
    {
        // The corpus's own example: a Speed of 30 feet spends 15 feet to mount a horse.
        var mounting = Value<MountingMove>(EntryPoints.MountingCost.Resolve(new MountingCostRequest
        {
            Action = MountAction.Mount,
            SpeedInFeet = 30,
            MovementLeftFeet = 30,
            DistanceToMountFeet = 5,
            Mount = Serves,
        }));

        Assert.Equal(15, mounting.CostFeet);
        Assert.True(mounting.Done);
        Assert.Equal(15, mounting.Deduction.MovementLeftFeet);
        Assert.Equal("Combat / Mounting and Dismounting / p. 15", mounting.Authority.Citation);
        Assert.Equal(EntryPoints.MountingCost.Registered.Locator, mounting.Authority);

        // An odd Speed: 25 feet costs 12, the fraction rounded down.
        var odd = Value<MountingMove>(EntryPoints.MountingCost.Resolve(new MountingCostRequest
        {
            Action = MountAction.Dismount,
            SpeedInFeet = 25,
            MovementLeftFeet = 25,
            DistanceToMountFeet = 0,
            Mount = Ridden,
        }));

        Assert.Equal(12, odd.CostFeet);
        Assert.True(odd.Done);
        Assert.Equal(13, odd.Deduction.MovementLeftFeet);
    }

    [Fact]
    public void A_rider_with_less_movement_left_than_the_cost_neither_mounts_nor_dismounts()
    {
        var mounting = Value<MountingMove>(EntryPoints.MountingCost.Resolve(new MountingCostRequest
        {
            Action = MountAction.Mount,
            SpeedInFeet = 30,
            MovementLeftFeet = 10,
            DistanceToMountFeet = 5,
            Mount = Serves,
        }));

        Assert.Equal(15, mounting.CostFeet);
        Assert.False(mounting.Done);
        Assert.True(mounting.Deduction.SpeedUsedUp);
        Assert.Equal(10, mounting.Deduction.MovementLeftFeet);
    }

    [Fact]
    public void A_creature_more_than_five_feet_away_is_not_mounted()
    {
        var mounting = Value<MountingMove>(EntryPoints.MountingCost.Resolve(new MountingCostRequest
        {
            Action = MountAction.Mount,
            SpeedInFeet = 30,
            MovementLeftFeet = 30,
            DistanceToMountFeet = 10,
            Mount = Serves,
        }));

        Assert.False(mounting.Done);
        Assert.Contains("within 5 feet", mounting.Because, StringComparison.Ordinal);
    }

    [Fact]
    public void A_mounted_combat_rule_declines_where_nothing_states_a_mount_or_a_rider()
    {
        var noMount = Declined(EntryPoints.MountingCost.Resolve(new MountingCostRequest
        {
            Action = MountAction.Mount,
            SpeedInFeet = 30,
            MovementLeftFeet = 30,
            DistanceToMountFeet = 5,
            Mount = NotAMount,
        }));

        Assert.Equal(UnresolvedReason.OutsideCurrentScope, noMount.Reason);
        Assert.Equal(EntryPoints.MountEligibility.Registered.Locator, noMount.Locator);
        Assert.Contains("'mounting-cost'", noMount.Attempted, StringComparison.Ordinal);

        // The four rules after mounting need a rider on the mount, which mounting-cost puts there.
        var notRidden = Declined(EntryPoints.ControlledMountTurn.Resolve(new ControlledMountTurnRequest
        {
            Creature = Horse,
            Mount = Serves,
        }));

        Assert.Equal(UnresolvedReason.OutsideCurrentScope, notRidden.Reason);
        Assert.Equal(EntryPoints.MountingCost.Registered.Locator, notRidden.Locator);
        Assert.Contains("'controlled-mount-turn'", notRidden.Attempted, StringComparison.Ordinal);

        var noRiderFalling = Declined(EntryPoints.FallingOff.Resolve(new FallingOffRequest
        {
            Trigger = FallTrigger.RiderKnockedProne,
            Mount = NotAMount,
        }));

        Assert.Equal(UnresolvedReason.OutsideCurrentScope, noRiderFalling.Reason);
        Assert.Equal(EntryPoints.MountEligibility.Registered.Locator, noRiderFalling.Locator);
    }

    [Fact]
    public void A_domesticated_horse_or_a_mule_has_the_training_a_controlled_mount_needs_citing_page_16()
    {
        foreach (var creature in new[] { Horse, MountCreatureStatement.Mule("the mule", Gm) })
        {
            var control = Value<MountControl>(EntryPoints.MountControlRequiresTraining.Resolve(
                new MountControlRequiresTrainingRequest { Creature = creature, Mount = Ridden }));

            Assert.True(control.CanBeControlled);
            Assert.Equal("Combat / Controlling a Mount / p. 16", control.Authority.Citation);
            Assert.Equal(EntryPoints.MountControlRequiresTraining.Registered.Locator, control.Authority);

            // The corpus names these two itself, so this answer is the corpus's and names no ruling.
            Assert.Empty(control.Rulings);
        }
    }

    [Fact]
    public void A_creature_the_corpus_does_not_name_is_trained_when_the_caller_states_it_naming_the_owners_ruling()
    {
        var trained = Value<MountControl>(EntryPoints.MountControlRequiresTraining.Resolve(
            new MountControlRequiresTrainingRequest
            {
                Creature = MountCreatureStatement.AnotherCreature("the giant elk", Gm, trained: true),
                Mount = Ridden,
            }));

        Assert.True(trained.CanBeControlled);

        var owners = Assert.Single(trained.Rulings);
        Assert.Equal("mount-control-requires-training/training-is-stated", owners.Id);
        Assert.Equal("Brandon", owners.RuledBy);
        Assert.Equal(new DateOnly(2026, 9, 16), owners.RuledOn);
        Assert.Equal("docs/decisions/0007-brandons-rulings-on-six-open-questions-are-ruleset-version-seven.md", owners.Record);

        // The caller may state the other way, and that rests on the ruling too.
        var untrained = Value<MountControl>(EntryPoints.MountControlRequiresTraining.Resolve(
            new MountControlRequiresTrainingRequest
            {
                Creature = MountCreatureStatement.AnotherCreature("the giant elk", Gm, trained: false),
                Mount = Ridden,
            }));

        Assert.False(untrained.CanBeControlled);
        Assert.Equal(owners, Assert.Single(untrained.Rulings));

        // The corpus's own instances are not the caller's to contradict.
        var contradicted = Assert.Throws<ArgumentException>(() =>
            new MountCreatureStatement(MountCreatureKind.DomesticatedHorse, "the horse", Gm, MountTraining.NotTrained));
        Assert.Equal("Training", contradicted.ParamName);
    }

    [Fact]
    public void With_no_stated_training_a_creature_the_corpus_does_not_name_declines()
    {
        var declined = Declined(EntryPoints.MountControlRequiresTraining.Resolve(
            new MountControlRequiresTrainingRequest
            {
                Creature = MountCreatureStatement.AnotherCreature("the giant elk", Gm),
                Mount = Ridden,
            }));

        Assert.Equal(UnresolvedReason.RequiresInterpretation, declined.Reason);
        Assert.Equal(EntryPoints.MountControlRequiresTraining.Registered.Locator, declined.Locator);
        Assert.Contains("'mount-control-requires-training'", declined.Attempted, StringComparison.Ordinal);
        Assert.Contains("similar creatures", declined.Attempted, StringComparison.Ordinal);

        // A decline relies on no ruling and names none (rules-factory 0027 § 4).
        Assert.DoesNotContain("training-is-stated", declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void A_controlled_mount_takes_the_riders_Initiative_and_has_only_Dash_Disengage_and_Dodge_citing_page_16()
    {
        var turn = Value<ControlledMountTurn>(EntryPoints.ControlledMountTurn.Resolve(new ControlledMountTurnRequest
        {
            Creature = Horse,
            Mount = Ridden,
        }));

        Assert.True(turn.InitiativeMatchesTheRider);
        Assert.True(turn.MovesOnYourTurnAsYouDirect);
        Assert.True(turn.ActsOnTheTurnItIsMounted);
        Assert.Equal(
            [ControlledMountAction.Dash, ControlledMountAction.Disengage, ControlledMountAction.Dodge],
            turn.ActionOptions.ToArray());
        Assert.Equal(Enum.GetValues<ControlledMountAction>(), turn.ActionOptions.ToArray());
        Assert.True(turn.Allows(ControlledMountAction.Dodge));
        Assert.Equal("Combat / Controlling a Mount / p. 16", turn.Authority.Citation);
        Assert.Equal(EntryPoints.ControlledMountTurn.Registered.Locator, turn.Authority);

        // A horse is trained by the corpus's own words, so this turn rests on no ruling.
        Assert.Empty(turn.Rulings);
    }

    [Fact]
    public void A_controlled_mount_trained_by_the_callers_statement_carries_that_ruling_into_its_turn()
    {
        var turn = Value<ControlledMountTurn>(EntryPoints.ControlledMountTurn.Resolve(new ControlledMountTurnRequest
        {
            Creature = MountCreatureStatement.AnotherCreature("the giant elk", Gm, trained: true),
            Mount = Ridden,
        }));

        Assert.True(turn.MovesOnYourTurnAsYouDirect);
        Assert.True(turn.Control.CanBeControlled);

        // This rule has no ruling of its own; it carries its input's (0027 § 4, as amended).
        Assert.Equal(OwnerRulings.TrainingIsAStatedFact, Assert.Single(turn.Rulings));
        Assert.Equal(turn.Control.Rulings.ToArray(), turn.Rulings.ToArray());

        // A mount the caller states is not trained is not controlled, on the same ruling.
        var untrained = Value<ControlledMountTurn>(EntryPoints.ControlledMountTurn.Resolve(new ControlledMountTurnRequest
        {
            Creature = MountCreatureStatement.AnotherCreature("the giant elk", Gm, trained: false),
            Mount = Ridden,
        }));

        Assert.False(untrained.MovesOnYourTurnAsYouDirect);
        Assert.Empty(untrained.ActionOptions);
        Assert.Equal(OwnerRulings.TrainingIsAStatedFact, Assert.Single(untrained.Rulings));
    }

    [Fact]
    public void What_one_of_the_three_action_options_does_declines_citing_the_Actions_table()
    {
        var declined = Declined(EntryPoints.ControlledMountTurn.Resolve(new ControlledMountTurnRequest
        {
            Creature = Horse,
            Mount = Ridden,
            AskingWhatAnActionDoes = ControlledMountAction.Dash,
        }));

        Assert.Equal(UnresolvedReason.OutsideCurrentScope, declined.Reason);
        Assert.Equal(EntryPoints.ActionsTable.Registered.Locator, declined.Locator);
        Assert.Contains("Dash", declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void A_controlled_mounts_turn_declines_where_nothing_states_the_mounts_training()
    {
        var declined = Declined(EntryPoints.ControlledMountTurn.Resolve(new ControlledMountTurnRequest
        {
            Creature = MountCreatureStatement.AnotherCreature("the giant elk", Gm),
            Mount = Ridden,
        }));

        Assert.Equal(UnresolvedReason.RequiresInterpretation, declined.Reason);
        Assert.Equal(EntryPoints.MountControlRequiresTraining.Registered.Locator, declined.Locator);
        Assert.Contains("'controlled-mount-turn'", declined.Attempted, StringComparison.Ordinal);
        Assert.Contains("trained to accept a rider", declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void An_independent_mount_retains_its_place_in_the_Initiative_order_citing_page_16()
    {
        var independent = Value<IndependentMount>(EntryPoints.IndependentMount.Resolve(new IndependentMountRequest
        {
            Behaviour = MountBehaviourStatement.IgnoresYourControl(Gm),
            Mount = Ridden,
        }));

        Assert.True(independent.Independent);
        Assert.True(independent.RetainsItsPlaceInTheInitiativeOrder);
        Assert.True(independent.MovesAndActsAsItLikes);
        Assert.Equal("Combat / Controlling a Mount / p. 16", independent.Authority.Citation);
        Assert.Equal(EntryPoints.IndependentMount.Registered.Locator, independent.Authority);
    }

    [Fact]
    public void A_mount_that_takes_the_riders_direction_is_not_independent()
    {
        var controlled = Value<IndependentMount>(EntryPoints.IndependentMount.Resolve(new IndependentMountRequest
        {
            Behaviour = MountBehaviourStatement.TakesYourDirection(Gm),
            Mount = Ridden,
        }));

        Assert.False(controlled.Independent);
        Assert.False(controlled.RetainsItsPlaceInTheInitiativeOrder);
        Assert.Contains("controlled-mount-turn", controlled.Because, StringComparison.Ordinal);
    }

    [Fact]
    public void What_makes_a_mount_independent_and_what_it_does_are_unresolved_and_decline()
    {
        var unstated = Declined(EntryPoints.IndependentMount.Resolve(new IndependentMountRequest
        {
            Behaviour = MountBehaviourStatement.NotStated(Gm),
            Mount = Ridden,
        }));

        Assert.Equal(UnresolvedReason.RequiresInterpretation, unstated.Reason);
        Assert.Equal(EntryPoints.IndependentMount.Registered.Locator, unstated.Locator);
        Assert.Contains("ignores your control", unstated.Attempted, StringComparison.Ordinal);

        var whatItDoes = Declined(EntryPoints.IndependentMount.Resolve(new IndependentMountRequest
        {
            Behaviour = MountBehaviourStatement.IgnoresYourControl(Gm),
            Mount = Ridden,
            AskingWhatItDoes = true,
        }));

        Assert.Equal(UnresolvedReason.RequiresInterpretation, whatItDoes.Reason);
        Assert.Contains("names no decider", whatItDoes.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void Each_of_the_three_cases_demands_a_DC_10_Dexterity_saving_throw_citing_page_16()
    {
        Assert.Equal(
            [FallTrigger.MountMovedAgainstItsWill, FallTrigger.RiderKnockedProne, FallTrigger.MountKnockedProne],
            Enum.GetValues<FallTrigger>());

        foreach (var trigger in Enum.GetValues<FallTrigger>())
        {
            var ruling = Value<FallingOffRuling>(EntryPoints.FallingOff.Resolve(new FallingOffRequest
            {
                Trigger = trigger,
                Mount = Ridden,
            }));

            Assert.Equal(10, ruling.SavingThrowDc);
            Assert.Equal("Dexterity", ruling.Ability);
            Assert.Null(ruling.StaysOn);
            Assert.Equal("Combat / Falling Off / p. 16", ruling.Authority.Citation);
            Assert.Equal(EntryPoints.SavingThrows.Registered.Locator, ruling.SaveAuthority);
        }
    }

    [Fact]
    public void A_successful_save_keeps_the_rider_on_the_mount()
    {
        var ruling = Value<FallingOffRuling>(EntryPoints.FallingOff.Resolve(new FallingOffRequest
        {
            Trigger = FallTrigger.MountMovedAgainstItsWill,
            Outcome = SaveOutcome.Succeeds(Gm),
            Mount = Ridden,
        }));

        Assert.True(ruling.StaysOn);
        Assert.Equal(10, ruling.SavingThrowDc);
    }

    [Fact]
    public void A_failed_save_declines_RequiresInterpretation_naming_the_space_the_rider_lands_in()
    {
        var declined = Declined(EntryPoints.FallingOff.Resolve(new FallingOffRequest
        {
            Trigger = FallTrigger.MountKnockedProne,
            Outcome = SaveOutcome.Fails(Gm),
            Mount = Ridden,
        }));

        Assert.Equal(UnresolvedReason.RequiresInterpretation, declined.Reason);
        Assert.Equal(EntryPoints.FallingOff.Registered.Locator, declined.Locator);
        Assert.Contains("'falling-off'", declined.Attempted, StringComparison.Ordinal);
        Assert.Contains("unoccupied space within 5 feet", declined.Attempted, StringComparison.Ordinal);
        Assert.Contains("prone-condition", declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void A_statement_a_mounted_combat_rule_needs_is_refused_when_left_out_never_inferred()
    {
        Assert.Throws<ArgumentException>(() => EntryPoints.MountEligibility.Resolve(new MountEligibilityRequest
        {
            Rider = CreatureSize.Medium,
            Candidate = "the warhorse",
            CandidateSize = CreatureSize.Large,
        }));
        Assert.Throws<ArgumentException>(() => EntryPoints.MountingCost.Resolve(new MountingCostRequest
        {
            Action = MountAction.Mount,
            SpeedInFeet = 30,
            MovementLeftFeet = 30,
            DistanceToMountFeet = 5,
        }));
        Assert.Throws<ArgumentException>(() => EntryPoints.MountControlRequiresTraining.Resolve(
            new MountControlRequiresTrainingRequest { Mount = Ridden }));
        Assert.Throws<ArgumentException>(() => EntryPoints.IndependentMount.Resolve(
            new IndependentMountRequest { Mount = Ridden }));
        Assert.Throws<ArgumentException>(() => EntryPoints.FallingOff.Resolve(
            new FallingOffRequest { Mount = Ridden }));
    }
}
