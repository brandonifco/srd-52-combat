using RulesKernel.Provenance;
using Srd52Combat.Initiative;

namespace Srd52Combat.Attacks;

/// <summary>
/// A reach greater than 5 feet, as the caller states it: "Certain creatures have melee attacks with
/// a reach greater than 5 feet, as noted in their descriptions" (<c>reach</c>, "Combat / Reach /
/// p. 15"). Stat blocks and the Reach weapon property are outside the extent, so a greater reach
/// reaches the engine as a parameter and is never inferred.
/// </summary>
/// <param name="Feet">The creature's reach, in feet; greater than the 5 feet the rule gives every creature.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record GreaterReach(int Feet, string StatedBy)
{
    /// <summary>The reach, checked to be greater than the 5 feet the rule states.</summary>
    public int Feet { get; } = Feet > TargetingValues.DefaultReachFeet
        ? Feet
        : throw new ArgumentOutOfRangeException(
            nameof(Feet),
            Feet,
            $"the rule states a {TargetingValues.DefaultReachFeet}-foot reach and names only a reach greater than it; "
            + "a reach of 5 feet or less is not a description the corpus provides for");

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <inheritdoc/>
    public override string ToString() =>
        $"the creature's description gives it a reach of {Feet} feet, as stated by {StatedBy}";
}

/// <summary>The figures the reach rules print.</summary>
public static class TargetingValues
{
    /// <summary>The reach every creature has, in feet: "A creature has a 5-foot reach".</summary>
    public const int DefaultReachFeet = 5;
}

/// <summary>
/// A creature's reach: <c>reach</c>, "Combat / Reach / p. 15". Five feet, unless the creature's
/// description gives it a greater one, which is a parameter.
/// </summary>
/// <param name="Feet">The reach, in feet.</param>
/// <param name="IsTheDefault">True when it is the 5 feet the rule gives every creature.</param>
/// <param name="Greater">The greater reach the caller stated, if any.</param>
/// <param name="Authority">The rule: "Combat / Reach / p. 15".</param>
public sealed record CreatureReach(int Feet, bool IsTheDefault, GreaterReach? Greater, SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        IsTheDefault
            ? $"a creature has a {Feet}-foot reach [{Authority.Citation}]"
            : $"this creature's reach is {Feet} feet, greater than the {TargetingValues.DefaultReachFeet} feet a creature has: {Greater} [{Authority.Citation}]";
}

/// <summary>
/// What <c>melee-within-reach</c> says of a melee attack at a distance: "A melee attack allows you
/// to attack a target within your reach", "Combat / Melee Attacks / p. 15".
/// </summary>
/// <param name="WithinReach">True when the target is within the attacker's reach.</param>
/// <param name="DistanceFeet">The distance to the target, in feet.</param>
/// <param name="Reach">The attacker's reach, as <c>reach</c> states it.</param>
/// <param name="Authority">The rule: "Combat / Melee Attacks / p. 15".</param>
public sealed record MeleeTargeting(bool WithinReach, int DistanceFeet, CreatureReach Reach, SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        WithinReach
            ? $"{DistanceFeet} ft is within the attacker's reach, so a melee attack may target it [{Authority.Citation}]; {Reach}"
            : $"{DistanceFeet} ft is beyond the attacker's reach, so a melee attack may not target it [{Authority.Citation}]; {Reach}";
}

/// <summary>
/// What <c>single-range</c> says of a ranged attack with one range: "If a ranged attack, such as
/// one made with a spell, has a single range, you can't attack a target beyond this range",
/// "Combat / Range / p. 15".
/// </summary>
/// <param name="CanAttack">True when the target is within the attack's range.</param>
/// <param name="RangeFeet">The attack's single range, in feet.</param>
/// <param name="DistanceFeet">The distance to the target, in feet.</param>
/// <param name="Authority">The rule: "Combat / Range / p. 15".</param>
public sealed record SingleRangeVerdict(bool CanAttack, int RangeFeet, int DistanceFeet, SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        CanAttack
            ? $"{DistanceFeet} ft is within the attack's range of {RangeFeet} ft, so it may be attacked [{Authority.Citation}]"
            : $"{DistanceFeet} ft is beyond the attack's range of {RangeFeet} ft, so it can't be attacked [{Authority.Citation}]";
}
