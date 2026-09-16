using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.AttackFixtures;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// "Combat / Making an Attack", pp. 14 and 15, through the entry points only: what makes an attack
/// (<c>attack-sources</c>), the three steps (<c>attack-structure</c>) and each of them
/// (<c>attack-target</c>, <c>attack-modifiers</c>, <c>attack-resolution</c>).
/// </summary>
public class MakingAnAttackEntryPointTests
{
    [Fact]
    public void The_Attack_action_makes_an_attack_citing_page_14()
    {
        var made = Value<AttackMade>(EntryPoints.AttackSources.Resolve(new AttackSourcesRequest
        {
            Source = AttackSource.AttackAction,
            Incapacitated = NotIncapacitated,
        }));

        Assert.Equal(AttackSource.AttackAction, made.Source);
        Assert.Equal("Combat / Making an Attack / p. 14", made.Authority.Citation);
        Assert.Equal(EntryPoints.AttackSources.Registered.Locator, made.Authority);
    }

    [Fact]
    public void Another_action_a_Bonus_Action_or_a_Reaction_declines_citing_the_entry_that_says_which_ones()
    {
        (AttackSource Source, string EntryId)[] cases =
        [
            (AttackSource.OtherAction, "actions-table"),
            (AttackSource.BonusAction, "bonus-actions"),
            (AttackSource.Reaction, "reactions"),
        ];

        foreach (var (source, entryId) in cases)
        {
            var declined = Declined(EntryPoints.AttackSources.Resolve(new AttackSourcesRequest
            {
                Source = source,
                Incapacitated = NotIncapacitated,
            }));

            Assert.Equal(UnresolvedReason.OutsideCurrentScope, declined.Reason);
            Assert.Equal(Registry.Entry(entryId).Locator, declined.Locator);
            Assert.Contains("'attack-sources'", declined.Attempted, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void An_Incapacitated_attacker_makes_no_attack_and_it_declines_citing_the_condition()
    {
        var declined = Declined(EntryPoints.AttackSources.Resolve(new AttackSourcesRequest
        {
            Source = AttackSource.AttackAction,
            Incapacitated = Incapacitated,
        }));

        Assert.Equal(UnresolvedReason.OutsideCurrentScope, declined.Reason);
        Assert.Equal(EntryPoints.IncapacitatedCondition.Registered.Locator, declined.Locator);
        Assert.Contains(Incapacitated.ToString(), declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void An_attack_is_three_steps_in_order_choose_a_target_determine_modifiers_resolve_citing_page_15()
    {
        var structure = Value<AttackStructure>(EntryPoints.AttackStructure.Resolve(AttackStructureRequest.Empty));

        Assert.Equal([1, 2, 3], structure.Steps.Select(s => s.Number));
        Assert.Equal(
            ["Choose a Target", "Determine Modifiers", "Resolve the Attack"],
            structure.Steps.Select(s => s.Name));
        Assert.Equal(
            ["attack-target", "attack-modifiers", "attack-resolution"],
            structure.Steps.Select(s => s.EntryId));
        Assert.Equal("Combat / Making an Attack / p. 15", structure.Authority.Citation);
        Assert.Equal(EntryPoints.AttackStructure.Registered.Locator, structure.Authority);
    }

    [Fact]
    public void A_creature_an_object_or_a_location_within_range_is_a_legal_target_citing_page_15()
    {
        // The set of target kinds is closed at three, and each of the three is a legal target.
        Assert.Equal([TargetKind.Creature, TargetKind.Object, TargetKind.Location], Enum.GetValues<TargetKind>());

        foreach (var kind in Enum.GetValues<TargetKind>())
        {
            var chosen = Value<ChosenTarget>(EntryPoints.AttackTarget.Resolve(new AttackTargetRequest
            {
                Kind = kind,
                Target = "the goblin behind the crate",
                RangeKind = AttackRangeKind.RangedTwoRanges,
                Ranges = Longbow,
                DistanceFeet = 100,
            }));

            Assert.Equal(kind, chosen.Kind);
            Assert.True(chosen.WithinRange);
            Assert.Equal(RangeBand.WithinNormalRange, chosen.Range.Band);
            Assert.Equal(RollEffect.None, chosen.Range.Effect);
            Assert.Equal("Combat / Making an Attack / p. 15", chosen.Authority.Citation);
            Assert.Equal("Combat / Range / p. 15", chosen.Range.Authority.Citation);
        }
    }

    [Fact]
    public void A_target_beyond_long_range_cannot_be_picked()
    {
        var chosen = Value<ChosenTarget>(EntryPoints.AttackTarget.Resolve(new AttackTargetRequest
        {
            Kind = TargetKind.Creature,
            Target = "the rider on the ridge",
            RangeKind = AttackRangeKind.RangedTwoRanges,
            Ranges = Longbow,
            DistanceFeet = Longbow.LongFeet + 1,
        }));

        Assert.False(chosen.WithinRange);
        Assert.Equal(RangeBand.BeyondLongRange, chosen.Range.Band);
        Assert.False(chosen.Range.CanAttack);
    }

    [Fact]
    public void A_melee_attack_or_a_single_range_attack_declines_citing_the_range_rule_that_is_not_built()
    {
        (AttackRangeKind RangeKind, string EntryId)[] cases =
        [
            (AttackRangeKind.Melee, "melee-within-reach"),
            (AttackRangeKind.RangedSingleRange, "single-range"),
        ];

        foreach (var (rangeKind, entryId) in cases)
        {
            var declined = Declined(EntryPoints.AttackTarget.Resolve(new AttackTargetRequest
            {
                Kind = TargetKind.Creature,
                Target = "the goblin",
                RangeKind = rangeKind,
                DistanceFeet = 5,
            }));

            Assert.Equal(UnresolvedReason.UnsupportedRule, declined.Reason);
            Assert.Equal(Registry.Entry(entryId).Locator, declined.Locator);
            Assert.Contains(entryId, declined.Attempted, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void A_target_kind_outside_the_three_is_refused()
    {
        var request = new AttackTargetRequest
        {
            Kind = (TargetKind)0,
            Target = "a rumour",
            RangeKind = AttackRangeKind.RangedTwoRanges,
            Ranges = Longbow,
            DistanceFeet = 10,
        };

        var error = Assert.Throws<ArgumentException>(() => EntryPoints.AttackTarget.Resolve(request));

        Assert.Contains("a creature, an object, or a location", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Determining_modifiers_declines_citing_cover_degree_and_names_what_it_determined()
    {
        var disadvantage = Value<RollDetermination>(EntryPoints.UnseenTargetDisadvantage.Resolve(
            new UnseenTargetDisadvantageRequest { Visibility = TargetVisibilityStatement.HeardNotSeen(Gm) }));

        var declined = Declined(EntryPoints.AttackModifiers.Resolve(new AttackModifiersRequest
        {
            Determinations = [disadvantage],
            Other = ["Bless adds 1d4 to the attack roll"],
        }));

        Assert.Equal(UnresolvedReason.UnsupportedRule, declined.Reason);
        Assert.Equal(EntryPoints.CoverDegree.Registered.Locator, declined.Locator);
        Assert.Contains("'attack-modifiers'", declined.Attempted, StringComparison.Ordinal);
        Assert.Contains("cover-degree", declined.Attempted, StringComparison.Ordinal);
        Assert.Contains(disadvantage.ToString(), declined.Attempted, StringComparison.Ordinal);
        Assert.Contains("Bless adds 1d4 to the attack roll", declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void On_a_hit_damage_is_rolled_and_the_rule_that_rolls_it_is_named_citing_page_15()
    {
        var resolved = Value<DamageOnHit>(EntryPoints.AttackResolution.Resolve(new AttackResolutionRequest
        {
            Outcome = AttackRollOutcome.Hits(Gm),
            DamageRules = OrdinaryDamage,
        }));

        Assert.True(resolved.Hit);
        Assert.True(resolved.DamageIsRolled);
        Assert.Equal("Combat / Making an Attack / p. 15", resolved.Authority.Citation);
        Assert.Equal(EntryPoints.DamageRolls.Registered.Locator, resolved.DamageAuthority);
    }

    [Fact]
    public void On_a_miss_no_damage_is_rolled()
    {
        var resolved = Value<DamageOnHit>(EntryPoints.AttackResolution.Resolve(new AttackResolutionRequest
        {
            Outcome = AttackRollOutcome.Misses(Gm),
            DamageRules = OrdinaryDamage,
        }));

        Assert.False(resolved.Hit);
        Assert.False(resolved.DamageIsRolled);
        Assert.Null(resolved.DamageAuthority);
    }

    [Fact]
    public void An_attack_whose_own_rules_specify_otherwise_rolls_no_damage_on_a_hit()
    {
        var otherwise = AttackDamageRules.SpecifyOtherwise(Gm);

        var resolved = Value<DamageOnHit>(EntryPoints.AttackResolution.Resolve(new AttackResolutionRequest
        {
            Outcome = AttackRollOutcome.Hits(Gm),
            DamageRules = otherwise,
        }));

        Assert.True(resolved.Hit);
        Assert.False(resolved.DamageIsRolled);
        Assert.Null(resolved.DamageAuthority);
        Assert.Contains(otherwise.ToString(), resolved.Because, StringComparison.Ordinal);
    }

    [Fact]
    public void A_statement_an_attack_step_needs_is_refused_when_left_out_never_inferred()
    {
        Assert.Throws<ArgumentException>(() => EntryPoints.AttackSources.Resolve(
            new AttackSourcesRequest { Source = AttackSource.AttackAction }));
        Assert.Throws<ArgumentException>(() => EntryPoints.AttackResolution.Resolve(
            new AttackResolutionRequest { Outcome = AttackRollOutcome.Hits(Gm) }));
        Assert.Throws<ArgumentException>(() => EntryPoints.AttackResolution.Resolve(
            new AttackResolutionRequest { DamageRules = OrdinaryDamage }));
        Assert.Throws<ArgumentException>(() => EntryPoints.AttackModifiers.Resolve(
            new AttackModifiersRequest { Determinations = [] }));
    }
}
