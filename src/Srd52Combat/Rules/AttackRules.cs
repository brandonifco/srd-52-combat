using System.Collections.Immutable;
using RulesKernel.Resolution;
using Srd52Combat.Attacks;

namespace Srd52Combat.Rules;

/// <summary>
/// Making an attack, "Combat / Making an Attack / p. 14" and "/ p. 15", with the sidebar and the
/// two range sections that bear on it: what makes an attack (<c>attack-sources</c>), its three
/// steps (<c>attack-structure</c>, <c>attack-target</c>, <c>attack-modifiers</c>,
/// <c>attack-resolution</c>), the unseen cases (<c>unseen-attacker-advantage</c>,
/// <c>unseen-target-disadvantage</c>, <c>wrong-location-misses</c>,
/// <c>hidden-attacker-revealed</c>) and range (<c>normal-and-long-range</c>,
/// <c>ranged-in-close-combat</c>).
/// </summary>
/// <remarks>
/// <para>
/// Two kinds of thing lie outside this engine's slice and are never guessed at. A rule the corpus
/// has and this engine has not built declines <see cref="UnresolvedReason.UnsupportedRule"/> citing
/// that entry; a rule outside the slice altogether declines
/// <see cref="UnresolvedReason.OutsideCurrentScope"/> citing it (rules-factory decision 0021). In
/// particular the d20 of an attack roll is <c>attack-rolls</c>' and its damage is
/// <c>damage-rolls</c>', so nothing here draws: the engine says what the modifiers and the outcome
/// are, and names the rule that rolls.
/// </para>
/// <para>
/// Advantage and Disadvantage are named, never combined: how two of them resolve is
/// <c>advantage-disadvantage</c> ("Playing the Game / p. 7"), <c>scope: out</c>. Each rule that
/// gives one answers with its own <see cref="RollDetermination"/>.
/// </para>
/// </remarks>
public static class AttackRules
{
    /// <summary>The distance within which <c>ranged-in-close-combat</c> looks for an enemy, in feet.</summary>
    public const int CloseCombatFeet = 5;

    /// <summary>
    /// Whether taking <paramref name="source"/> makes an attack: "When you take the Attack action,
    /// you make an attack. Some other actions, Bonus Actions, and Reactions also let you make an
    /// attack." Which other ones do is the Actions table's, <c>bonus-actions</c>' and
    /// <c>reactions</c>', all outside the slice, so each of those declines.
    /// </summary>
    /// <param name="source">What the attacker takes.</param>
    /// <param name="incapacitated">Whether the attacker has the Incapacitated condition. Never defaulted.</param>
    /// <returns>That an attack is made, or the decline.</returns>
    public static Resolution<AttackMade> Sources(AttackSource source, IncapacitatedStatement incapacitated)
    {
        ArgumentNullException.ThrowIfNull(incapacitated);
        if (!Enum.IsDefined(source))
        {
            throw new ArgumentException($"the attack's source must be stated; {source} is not an {nameof(AttackSource)}", nameof(source));
        }

        string attempting = Attempting(MapEntries.AttackSources);
        if (incapacitated.Incapacitated)
        {
            return Decline<AttackMade>(
                UnresolvedReason.OutsideCurrentScope,
                $"{attempting} while {incapacitated}: an Incapacitated creature can take no action, Bonus Action or Reaction, "
                + "and every source of an attack this rule names is one of those",
                MapEntries.IncapacitatedCondition);
        }

        return source switch
        {
            AttackSource.AttackAction =>
                Resolution<AttackMade>.FromValue(new AttackMade(source, MapEntries.AttackSources.Locator)),
            AttackSource.OtherAction => Decline<AttackMade>(
                UnresolvedReason.OutsideCurrentScope,
                $"{attempting} for another action: the corpus says some other actions also let you make an attack, and which ones is the Actions table's",
                MapEntries.ActionsTable),
            AttackSource.BonusAction => Decline<AttackMade>(
                UnresolvedReason.OutsideCurrentScope,
                $"{attempting} for a Bonus Action: the corpus says some Bonus Actions also let you make an attack, and which ones is '{MapEntries.BonusActions.Id}'",
                MapEntries.BonusActions),
            _ => Decline<AttackMade>(
                UnresolvedReason.OutsideCurrentScope,
                $"{attempting} for a Reaction: the corpus says some Reactions also let you make an attack, and which ones is '{MapEntries.Reactions.Id}'",
                MapEntries.Reactions),
        };
    }

    /// <summary>
    /// The structure of an attack: "an attack has the following structure: 1: Choose a Target.
    /// 2: Determine Modifiers. 3: Resolve the Attack." Each step is its own map entry, which the
    /// caller resolves in turn; this rule is the order, and nothing else.
    /// </summary>
    /// <returns>The three steps, in order.</returns>
    public static Resolution<AttackStructure> Structure() =>
        Resolution<AttackStructure>.FromValue(new AttackStructure(
            [
                new AttackStep(1, "Choose a Target", MapEntries.AttackTarget.Id),
                new AttackStep(2, "Determine Modifiers", MapEntries.AttackModifiers.Id),
                new AttackStep(3, "Resolve the Attack", MapEntries.AttackResolution.Id),
            ],
            MapEntries.AttackStructure.Locator));

    /// <summary>
    /// Step 1: "Pick a target within your attack's range: a creature, an object, or a location."
    /// The three kinds are the whole set. Whether the target is within range is the range rule of
    /// the attack the caller states, and exactly one of the three applies: <c>normal-and-long-range</c>
    /// for a ranged attack with two ranges, <c>single-range</c> for a ranged attack with one, and
    /// <c>melee-within-reach</c> for a melee attack. All three are built.
    /// </summary>
    /// <param name="kind">A creature, an object, or a location.</param>
    /// <param name="target">The target's name or handle.</param>
    /// <param name="rangeKind">Which range rule the attack is measured by.</param>
    /// <param name="ranges">The attack's two ranges, for a ranged attack that has them.</param>
    /// <param name="distanceFeet">The distance to the target, in feet.</param>
    /// <param name="singleRangeFeet">The attack's single range in feet, for a ranged attack that has one.</param>
    /// <param name="greaterReach">A reach greater than 5 feet, for a melee attack by a creature whose description gives it one.</param>
    /// <returns>The chosen target, or the decline.</returns>
    public static Resolution<ChosenTarget> Target(
        TargetKind kind,
        string target,
        AttackRangeKind rangeKind,
        TwoRanges? ranges,
        int distanceFeet,
        int? singleRangeFeet = null,
        GreaterReach? greaterReach = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(target);
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentException(
                $"a target is a creature, an object, or a location; {kind} is not a {nameof(TargetKind)}", nameof(kind));
        }

        if (!Enum.IsDefined(rangeKind))
        {
            throw new ArgumentException(
                $"the attack's range rule must be stated; {rangeKind} is not an {nameof(AttackRangeKind)}", nameof(rangeKind));
        }

        switch (rangeKind)
        {
            case AttackRangeKind.Melee:
                var melee = TargetingRules.Melee(distanceFeet, greaterReach);
                return Resolution<ChosenTarget>.FromValue(new ChosenTarget(
                    kind, target, melee.WithinReach, null, melee, null, MapEntries.AttackTarget.Locator));
            case AttackRangeKind.RangedSingleRange:
                int range = singleRangeFeet ?? throw new ArgumentNullException(
                    nameof(singleRangeFeet), "a ranged attack with a single range is measured by it, and it was not given");
                var single = TargetingRules.SingleRange(range, distanceFeet);
                return Resolution<ChosenTarget>.FromValue(new ChosenTarget(
                    kind, target, single.CanAttack, null, null, single, MapEntries.AttackTarget.Locator));
            default:
                var two = ranges ?? throw new ArgumentNullException(
                    nameof(ranges), "a ranged attack with two ranges is measured by both of them, and they were not given");
                return Range(two, distanceFeet).Match(
                    verdict => Resolution<ChosenTarget>.FromValue(
                        new ChosenTarget(kind, target, verdict.CanAttack, verdict, null, null, MapEntries.AttackTarget.Locator)),
                    Resolution<ChosenTarget>.FromUnresolved);
        }
    }

    /// <summary>
    /// <c>normal-and-long-range</c>: "Your attack roll has Disadvantage when your target is beyond
    /// normal range, and you can't attack a target beyond long range." "Beyond" is strict, so a
    /// target at exactly the normal range takes no Disadvantage and one at exactly the long range
    /// can still be attacked.
    /// </summary>
    /// <param name="ranges">The attack's normal and long ranges.</param>
    /// <param name="distanceFeet">The distance to the target, in feet.</param>
    /// <returns>Which band the distance falls in, and what it does to the roll.</returns>
    public static Resolution<RangeVerdict> Range(TwoRanges ranges, int distanceFeet)
    {
        ArgumentNullException.ThrowIfNull(ranges);
        ArgumentOutOfRangeException.ThrowIfNegative(distanceFeet);
        var (band, effect, canAttack) = distanceFeet <= ranges.NormalFeet
            ? (RangeBand.WithinNormalRange, RollEffect.None, true)
            : distanceFeet <= ranges.LongFeet
                ? (RangeBand.BeyondNormalRange, RollEffect.Disadvantage, true)
                : (RangeBand.BeyondLongRange, RollEffect.None, false);
        return Resolution<RangeVerdict>.FromValue(
            new RangeVerdict(band, canAttack, effect, ranges, distanceFeet, MapEntries.NormalAndLongRange.Locator));
    }

    /// <summary>
    /// <c>unseen-attacker-advantage</c>: "When a creature can't see you, you have Advantage on
    /// attack rolls against it."
    /// </summary>
    /// <param name="seen">Whether the creature can see the attacker. Never defaulted.</param>
    /// <returns>The determination.</returns>
    public static Resolution<RollDetermination> UnseenAttacker(AttackerSeenStatement seen)
    {
        ArgumentNullException.ThrowIfNull(seen);
        return Resolution<RollDetermination>.FromValue(new RollDetermination(
            seen.CanSeeYou ? RollEffect.None : RollEffect.Advantage,
            MapEntries.UnseenAttackerAdvantage.Id,
            seen.ToString(),
            MapEntries.UnseenAttackerAdvantage.Locator));
    }

    /// <summary>
    /// <c>unseen-target-disadvantage</c>: "When you make an attack roll against a target you can't
    /// see, you have Disadvantage on the roll. This is true whether you're guessing the target's
    /// location or targeting a creature you can hear but not see." Both of those are a target you
    /// can't see, and both take Disadvantage.
    /// </summary>
    /// <param name="visibility">Whether the attacker can see the target. Never defaulted.</param>
    /// <returns>The determination.</returns>
    public static Resolution<RollDetermination> UnseenTarget(TargetVisibilityStatement visibility)
    {
        ArgumentNullException.ThrowIfNull(visibility);
        return Resolution<RollDetermination>.FromValue(new RollDetermination(
            visibility.CannotSee ? RollEffect.Disadvantage : RollEffect.None,
            MapEntries.UnseenTargetDisadvantage.Id,
            visibility.ToString(),
            MapEntries.UnseenTargetDisadvantage.Locator));
    }

    /// <summary>
    /// <c>ranged-in-close-combat</c>: "you have Disadvantage on the roll if you are within 5 feet
    /// of an enemy who can see you and doesn't have the Incapacitated condition." All three must
    /// hold of the same enemy.
    /// </summary>
    /// <param name="enemies">Every enemy within 5 feet, as the caller states them. Never defaulted.</param>
    /// <returns>The determination.</returns>
    public static Resolution<RollDetermination> RangedInCloseCombat(EnemiesWithinFiveFeet enemies)
    {
        ArgumentNullException.ThrowIfNull(enemies);
        var threatening = enemies.Enemies.Where(e => e.CanSeeYou && !e.Incapacitated).ToImmutableArray();
        return Resolution<RollDetermination>.FromValue(new RollDetermination(
            threatening.IsEmpty ? RollEffect.None : RollEffect.Disadvantage,
            MapEntries.RangedInCloseCombat.Id,
            threatening.IsEmpty
                ? $"no enemy within {CloseCombatFeet} feet both can see the attacker and lacks the Incapacitated condition; {enemies}"
                : $"{string.Join(", ", threatening.Select(e => e.Id))} is within {CloseCombatFeet} feet, can see the attacker and does not have the Incapacitated condition; {enemies}",
            MapEntries.RangedInCloseCombat.Locator));
    }

    /// <summary>
    /// Step 2: "The GM determines whether the target has Cover and whether you have Advantage or
    /// Disadvantage against the target. In addition, spells, special abilities, and other effects
    /// can apply penalties or bonuses to your attack roll."
    /// </summary>
    /// <remarks>
    /// The Advantage and Disadvantage half is the rules of this slice, which the caller resolves and
    /// hands in, and the penalties and bonuses are each effect's own, outside the extent, so the
    /// caller states them. The Cover half is <c>cover-degree</c>, which this engine now builds: the
    /// step reads it rather than deciding Cover itself, and where that rule declines — a creature
    /// covering less than half of the target, or more than one source of cover — the step declines
    /// with it, for the same reason and citing the same entry, with everything it did determine
    /// named in <see cref="UnresolvedResult.Attempted"/>.
    /// </remarks>
    /// <param name="obstacles">What the caller states lies between attacker and target (<c>cover-degree</c>).</param>
    /// <param name="determinations">What the rules of this slice gave the roll.</param>
    /// <param name="other">Penalties and bonuses from spells, special abilities and other effects, as the caller states them.</param>
    /// <returns>The modifiers the step determines, or the decline <c>cover-degree</c> gave.</returns>
    public static Resolution<AttackModifiers> Modifiers(
        IReadOnlyList<Srd52Combat.Cover.CoveringObstacle> obstacles,
        IReadOnlyList<RollDetermination> determinations,
        IReadOnlyList<string> other)
    {
        ArgumentNullException.ThrowIfNull(obstacles);
        ArgumentNullException.ThrowIfNull(determinations);
        ArgumentNullException.ThrowIfNull(other);
        if (determinations.Any(d => d is null))
        {
            throw new ArgumentException("no determination is null", nameof(determinations));
        }

        if (other.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("every other modifier is named", nameof(other));
        }

        string determined = determinations.Count == 0
            ? "no rule of this slice gave the roll Advantage or Disadvantage"
            : string.Join("; ", determinations);
        string effects = other.Count == 0 ? "no other effect applies a penalty or bonus" : string.Join("; ", other);
        return CoverRules.Degree(obstacles).Match(
            cover => Resolution<AttackModifiers>.FromValue(new AttackModifiers(
                cover,
                [.. determinations],
                [.. other],
                MapEntries.AttackModifiers.Locator)),
            unresolved => Resolution<AttackModifiers>.FromUnresolved(new UnresolvedResult(
                unresolved.Reason,
                $"{Attempting(MapEntries.AttackModifiers)}, having determined {determined}, and {effects}: "
                + $"whether the target has Cover is '{MapEntries.CoverDegree.Id}', and {unresolved.Attempted}",
                unresolved.Locator)));
    }

    /// <summary>
    /// Step 3: "Make the attack roll. On a hit, you roll damage unless the particular attack has
    /// rules that specify otherwise." The roll is <c>attack-rolls</c>' and the damage roll
    /// <c>damage-rolls</c>', both outside the slice; what this rule says is that damage is rolled
    /// on a hit, and only on a hit, unless the attack's own rules specify otherwise.
    /// </summary>
    /// <param name="outcome">Whether the attack roll hit. Never defaulted.</param>
    /// <param name="damageRules">Whether the attack's own rules specify otherwise. Never defaulted.</param>
    /// <returns>Whether damage is rolled, and by which rule.</returns>
    public static Resolution<DamageOnHit> Resolve(AttackRollOutcome outcome, AttackDamageRules damageRules)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(damageRules);
        if (!outcome.Hit)
        {
            return Resolution<DamageOnHit>.FromValue(new DamageOnHit(
                Hit: false,
                DamageIsRolled: false,
                $"damage is rolled on a hit, and {outcome}",
                MapEntries.AttackResolution.Locator,
                DamageAuthority: null));
        }

        return damageRules.SpecifiesOtherwise
            ? Resolution<DamageOnHit>.FromValue(new DamageOnHit(
                Hit: true,
                DamageIsRolled: false,
                $"on a hit you roll damage unless the particular attack has rules that specify otherwise, and {damageRules}",
                MapEntries.AttackResolution.Locator,
                DamageAuthority: null))
            : Resolution<DamageOnHit>.FromValue(new DamageOnHit(
                Hit: true,
                DamageIsRolled: true,
                $"{outcome}, and {damageRules}",
                MapEntries.AttackResolution.Locator,
                MapEntries.DamageRolls.Locator));
    }

    /// <summary>
    /// <c>wrong-location-misses</c>: "If the target isn't in the location you targeted, you miss."
    /// The attack roll is still made, and the attack misses whatever it shows.
    /// </summary>
    /// <param name="location">The location targeted, and whether the target is in it. Never defaulted.</param>
    /// <returns>Whether the attack misses.</returns>
    public static Resolution<LocationAttack> WrongLocation(TargetLocationStatement location)
    {
        ArgumentNullException.ThrowIfNull(location);
        return Resolution<LocationAttack>.FromValue(new LocationAttack(
            !location.TargetIsThere,
            location.TargetedLocation,
            location.ToString(),
            MapEntries.WrongLocationMisses.Locator));
    }

    /// <summary>
    /// <c>hidden-attacker-revealed</c>: "If you are hidden when you make an attack roll, you give
    /// away your location when the attack hits or misses." Either outcome gives it away.
    /// </summary>
    /// <param name="hidden">Whether the attacker is hidden. Never defaulted.</param>
    /// <param name="outcome">Whether the attack roll hit. Never defaulted.</param>
    /// <returns>Whether the attacker's location is given away.</returns>
    public static Resolution<HiddenAttacker> Revealed(HiddenStatement hidden, AttackRollOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(hidden);
        ArgumentNullException.ThrowIfNull(outcome);
        return Resolution<HiddenAttacker>.FromValue(new HiddenAttacker(
            hidden.Hidden,
            hidden.Hidden
                ? $"{hidden}, and {outcome}"
                : $"{hidden}, so there is no hidden location to give away",
            MapEntries.HiddenAttackerRevealed.Locator));
    }

    internal static string Attempting(MapEntry entry) =>
        $"resolve the map entry '{entry.Id}' [{entry.Locator.Citation}]";

    internal static Resolution<T> Decline<T>(UnresolvedReason reason, string attempted, MapEntry cited)
        where T : notnull =>
        Resolution<T>.FromUnresolved(new UnresolvedResult(reason, attempted, cited.Locator));
}
