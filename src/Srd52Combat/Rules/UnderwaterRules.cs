using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Underwater;

namespace Srd52Combat.Rules;

/// <summary>
/// Underwater combat: the impeded weapons of "Combat / Impeded Weapons / p. 16"
/// (<c>underwater-melee</c>, <c>underwater-ranged</c>) and the Fire Resistance of "Combat / Fire
/// Resistance / p. 16" (<c>underwater-fire-resistance</c>).
/// </summary>
/// <remarks>
/// Whether the fight is underwater, whether the attacker has a Swim Speed, and what damage the
/// weapon deals are facts the caller states (rules-factory decision 0025). As in the attacks batch,
/// Advantage and Disadvantage are named and never combined: that is
/// <c>advantage-disadvantage</c>'s, <c>scope: out</c>, which both impeded-weapon entries name in
/// <c>dependsOn</c>. What Resistance itself does is <c>resistance</c>, also outside the extent, and
/// this engine names it rather than applying it. Two of the three entries cite "Combat / Impeded
/// Weapons / p. 16", so each decline names its entry.
/// </remarks>
public static class UnderwaterRules
{
    /// <summary>
    /// <c>underwater-melee</c>: "When making a melee attack roll with a weapon underwater, a
    /// creature that lacks a Swim Speed has Disadvantage on the attack roll unless the weapon deals
    /// Piercing damage."
    /// </summary>
    /// <remarks>
    /// All three conditions must hold for the Disadvantage: underwater, no Swim Speed, and a weapon
    /// that does not deal Piercing damage. A creature with a Swim Speed takes none, and neither does
    /// a Piercing weapon in the hands of one without.
    /// </remarks>
    /// <param name="where">Whether the fight is underwater, as the caller states it.</param>
    /// <param name="swimSpeed">Whether the attacker has a Swim Speed, as the caller states it.</param>
    /// <param name="weapon">The weapon and its damage, as the caller states them.</param>
    /// <returns>The determination this rule makes about the attack roll.</returns>
    public static Resolution<RollDetermination> Melee(
        UnderwaterStatement where,
        SwimSpeedStatement swimSpeed,
        WeaponStatement weapon)
    {
        ArgumentNullException.ThrowIfNull(where);
        ArgumentNullException.ThrowIfNull(swimSpeed);
        ArgumentNullException.ThrowIfNull(weapon);

        bool impeded = where.Underwater && !swimSpeed.HasSwimSpeed && !weapon.DealsPiercingDamage;
        string because = !where.Underwater
            ? $"the rule applies to a melee attack roll made underwater, and {where}"
            : swimSpeed.HasSwimSpeed
                ? $"the rule gives Disadvantage to a creature that lacks a Swim Speed, and {swimSpeed}"
                : weapon.DealsPiercingDamage
                    ? $"{where}, and {swimSpeed}, but the rule excepts a weapon that deals Piercing damage, and {weapon}"
                    : $"{where}, {swimSpeed}, and {weapon}";

        return Resolution<RollDetermination>.FromValue(new RollDetermination(
            impeded ? RollEffect.Disadvantage : RollEffect.None,
            MapEntries.UnderwaterMelee.Id,
            because,
            MapEntries.UnderwaterMelee.Locator));
    }

    /// <summary>
    /// <c>underwater-ranged</c>: "A ranged attack roll with a weapon underwater automatically misses
    /// a target beyond the weapon's normal range, and the attack roll has Disadvantage against a
    /// target within normal range."
    /// </summary>
    /// <remarks>
    /// The rule applies whether or not the attacker has a Swim Speed, and it replaces the
    /// long-range band of <c>normal-and-long-range</c> with an automatic miss. Where the target is
    /// against the weapon's two ranges is that entry's, read here rather than written again: beyond
    /// long range it also says the attack can't be made at all, and this rule says a shot at such a
    /// target misses; the engine reports both and chooses between them nowhere.
    /// </remarks>
    /// <param name="where">Whether the fight is underwater, as the caller states it.</param>
    /// <param name="ranges">The weapon's normal and long ranges.</param>
    /// <param name="distanceFeet">The distance to the target, in feet.</param>
    /// <returns>Whether the attack automatically misses, and what the rule does to the roll.</returns>
    public static Resolution<UnderwaterRangedAttack> Ranged(
        UnderwaterStatement where,
        TwoRanges ranges,
        int distanceFeet)
    {
        ArgumentNullException.ThrowIfNull(where);
        ArgumentNullException.ThrowIfNull(ranges);

        return AttackRules.Range(ranges, distanceFeet).Match(
            verdict =>
            {
                bool beyondNormal = distanceFeet > ranges.NormalFeet;
                bool misses = where.Underwater && beyondNormal;
                var effect = where.Underwater && !beyondNormal ? RollEffect.Disadvantage : RollEffect.None;
                string because = !where.Underwater
                    ? $"the rule applies to a ranged attack roll made underwater, and {where}"
                    : beyondNormal
                        ? $"{where}, and {distanceFeet} ft is beyond the weapon's normal range of {ranges.NormalFeet} ft"
                        : $"{where}, and {distanceFeet} ft is within the weapon's normal range of {ranges.NormalFeet} ft";

                return Resolution<UnderwaterRangedAttack>.FromValue(new UnderwaterRangedAttack(
                    misses,
                    effect,
                    verdict,
                    because,
                    MapEntries.UnderwaterRanged.Locator));
            },
            Resolution<UnderwaterRangedAttack>.FromUnresolved);
    }

    /// <summary>
    /// <c>underwater-fire-resistance</c>: "Anything underwater has Resistance to Fire damage
    /// (explained in 'Damage and Healing')."
    /// </summary>
    /// <remarks>
    /// "Anything" is the rule's own word: the engine asks nothing else of what is underwater. What
    /// Resistance does to the damage is <c>resistance</c> ("Resistance and Vulnerability", p. 17),
    /// outside this engine's extent, and the answer names it as the authority for that.
    /// </remarks>
    /// <param name="where">Whether the thing is underwater, as the caller states it.</param>
    /// <returns>Whether it has Resistance to Fire damage.</returns>
    public static Resolution<FireResistance> Fire(UnderwaterStatement where)
    {
        ArgumentNullException.ThrowIfNull(where);
        return Resolution<FireResistance>.FromValue(new FireResistance(
            where.Underwater,
            where,
            where.Underwater
                ? $"anything underwater has Resistance to Fire damage, and {where}"
                : $"the rule gives Resistance to Fire damage to anything underwater, and {where}",
            MapEntries.UnderwaterFireResistance.Locator,
            where.Underwater ? MapEntries.Resistance.Locator : null));
    }
}
