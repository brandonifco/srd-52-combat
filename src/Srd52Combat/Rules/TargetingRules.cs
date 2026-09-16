using Srd52Combat.Attacks;

namespace Srd52Combat.Rules;

/// <summary>
/// What an attack may reach: a creature's reach (<c>reach</c>, "Combat / Reach / p. 15"), the melee
/// attack it measures (<c>melee-within-reach</c>, "Combat / Melee Attacks / p. 15"), and the range
/// of a ranged attack that has only one (<c>single-range</c>, "Combat / Range / p. 15").
/// </summary>
/// <remarks>
/// These are the two alternatives to <c>normal-and-long-range</c> that <c>attack-target</c>'s
/// <c>dependsOn</c> names: exactly one of the three applies to a given attack, decided by what the
/// attack is, which the caller states. Whether an attack is melee or ranged, how far away the target
/// is, and a reach greater than the 5 feet the rule gives every creature are all parameters
/// (rules-factory decision 0025); the engine never infers one.
/// </remarks>
public static class TargetingRules
{
    /// <summary>
    /// <c>reach</c>: "A creature has a 5-foot reach and can thus attack targets within 5 feet when
    /// making a melee attack. Certain creatures have melee attacks with a reach greater than 5
    /// feet, as noted in their descriptions."
    /// </summary>
    /// <param name="greater">A greater reach the creature's description gives it, if the caller states one.</param>
    /// <returns>The creature's reach.</returns>
    public static CreatureReach Reach(GreaterReach? greater) =>
        greater is null
            ? new CreatureReach(TargetingValues.DefaultReachFeet, IsTheDefault: true, null, MapEntries.Reach.Locator)
            : new CreatureReach(greater.Feet, IsTheDefault: false, greater, MapEntries.Reach.Locator);

    /// <summary>
    /// <c>melee-within-reach</c>: "A melee attack allows you to attack a target within your reach."
    /// The reach is <see cref="Reach"/>'s, read here rather than written again.
    /// </summary>
    /// <param name="distanceFeet">The distance to the target, in feet.</param>
    /// <param name="greater">A greater reach the attacker's description gives it, if the caller states one.</param>
    /// <returns>Whether the target is within reach.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="distanceFeet"/> is negative.</exception>
    public static MeleeTargeting Melee(int distanceFeet, GreaterReach? greater)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(distanceFeet);
        var reach = Reach(greater);
        return new MeleeTargeting(
            distanceFeet <= reach.Feet,
            distanceFeet,
            reach,
            MapEntries.MeleeWithinReach.Locator);
    }

    /// <summary>
    /// <c>single-range</c>: "You can make ranged attacks only against targets within a specified
    /// range. If a ranged attack, such as one made with a spell, has a single range, you can't
    /// attack a target beyond this range." "Beyond" is strict, as it is in
    /// <c>normal-and-long-range</c>: a target at exactly the range can be attacked.
    /// </summary>
    /// <param name="rangeFeet">The attack's single range, in feet, as the weapon or spell states it.</param>
    /// <param name="distanceFeet">The distance to the target, in feet.</param>
    /// <returns>Whether the target is within the range.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rangeFeet"/> is not positive, or <paramref name="distanceFeet"/> is negative.</exception>
    public static SingleRangeVerdict SingleRange(int rangeFeet, int distanceFeet)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rangeFeet);
        ArgumentOutOfRangeException.ThrowIfNegative(distanceFeet);
        return new SingleRangeVerdict(
            distanceFeet <= rangeFeet,
            rangeFeet,
            distanceFeet,
            MapEntries.SingleRange.Locator);
    }
}
