using RulesKernel.Provenance;
using Srd52Combat.Attacks;
using Srd52Combat.Initiative;

namespace Srd52Combat.Underwater;

/// <summary>
/// Whether the fight is underwater, as the caller states it: "Combat / Impeded Weapons / p. 16" and
/// "Combat / Fire Resistance / p. 16" say what happens underwater and nothing about how a creature
/// comes to be there, so the engine is told and never infers it.
/// </summary>
/// <param name="Underwater">True when the creature or thing is underwater.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record UnderwaterStatement(bool Underwater, string StatedBy)
{
    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>It is underwater.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static UnderwaterStatement Is(string statedBy) => new(Underwater: true, statedBy);

    /// <summary>It is not underwater.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static UnderwaterStatement IsNot(string statedBy) => new(Underwater: false, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        Underwater
            ? $"it is underwater, as stated by {StatedBy}"
            : $"it is not underwater, as stated by {StatedBy}";
}

/// <summary>
/// Whether the attacker has a Swim Speed, as the caller states it. Special speeds are the Rules
/// Glossary's (<c>speed-and-size-sources</c> names where Speed comes from), outside this engine's
/// extent, so the engine is told and never infers one.
/// </summary>
/// <param name="HasSwimSpeed">True when the creature has a Swim Speed.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record SwimSpeedStatement(bool HasSwimSpeed, string StatedBy)
{
    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The creature has a Swim Speed.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static SwimSpeedStatement Has(string statedBy) => new(HasSwimSpeed: true, statedBy);

    /// <summary>The creature lacks a Swim Speed.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static SwimSpeedStatement Lacks(string statedBy) => new(HasSwimSpeed: false, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        HasSwimSpeed
            ? $"the creature has a Swim Speed, as stated by {StatedBy}"
            : $"the creature lacks a Swim Speed, as stated by {StatedBy}";
}

/// <summary>
/// The weapon a melee attack is made with, and whether it deals Piercing damage, as the caller
/// states it: "unless the weapon deals Piercing damage" (<c>underwater-melee</c>). Damage types are
/// "Damage and Healing" (p. 16 onward), outside the extent.
/// </summary>
/// <param name="Weapon">The weapon's name.</param>
/// <param name="DealsPiercingDamage">True when it deals Piercing damage.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record WeaponStatement(string Weapon, bool DealsPiercingDamage, string StatedBy)
{
    /// <summary>The weapon, checked to be named.</summary>
    public string Weapon { get; } = Checks.Text(Weapon, nameof(Weapon));

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>A weapon that deals Piercing damage.</summary>
    /// <param name="weapon">The weapon's name.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static WeaponStatement Piercing(string weapon, string statedBy) =>
        new(weapon, DealsPiercingDamage: true, statedBy);

    /// <summary>A weapon that deals damage of another type.</summary>
    /// <param name="weapon">The weapon's name.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static WeaponStatement NotPiercing(string weapon, string statedBy) =>
        new(weapon, DealsPiercingDamage: false, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        DealsPiercingDamage
            ? $"the {Weapon} deals Piercing damage, as stated by {StatedBy}"
            : $"the {Weapon} does not deal Piercing damage, as stated by {StatedBy}";
}

/// <summary>
/// What <c>underwater-ranged</c> makes of a ranged weapon attack: "A ranged attack roll with a
/// weapon underwater automatically misses a target beyond the weapon's normal range, and the attack
/// roll has Disadvantage against a target within normal range", "Combat / Impeded Weapons / p. 16".
/// </summary>
/// <param name="AutomaticallyMisses">True for a target beyond the weapon's normal range.</param>
/// <param name="Effect">What the rule does to the attack roll: Disadvantage within normal range.</param>
/// <param name="Range">Where the target stands against the weapon's two ranges (<c>normal-and-long-range</c>).</param>
/// <param name="Because">Why, in the engine's words, naming the statements it rests on.</param>
/// <param name="Authority">The rule: "Combat / Impeded Weapons / p. 16".</param>
public sealed record UnderwaterRangedAttack(
    bool AutomaticallyMisses,
    RollEffect Effect,
    RangeVerdict Range,
    string Because,
    SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        AutomaticallyMisses
            ? $"the attack automatically misses: {Because} [{Authority.Citation}]; {Range}"
            : $"the attack roll has {Effect}: {Because} [{Authority.Citation}]; {Range}";
}

/// <summary>
/// What <c>underwater-fire-resistance</c> says: "Anything underwater has Resistance to Fire damage
/// (explained in 'Damage and Healing')", "Combat / Fire Resistance / p. 16".
/// </summary>
/// <param name="HasResistanceToFireDamage">True when the thing is underwater.</param>
/// <param name="Where">What the caller stated about being underwater.</param>
/// <param name="Because">Why, in the engine's words.</param>
/// <param name="Authority">The rule: "Combat / Fire Resistance / p. 16".</param>
/// <param name="ResistanceAuthority">What Resistance does: <c>resistance</c>, outside this engine's extent; null when there is none.</param>
public sealed record FireResistance(
    bool HasResistanceToFireDamage,
    UnderwaterStatement Where,
    string Because,
    SourceLocator Authority,
    SourceLocator? ResistanceAuthority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        HasResistanceToFireDamage
            ? $"it has Resistance to Fire damage [{Authority.Citation}], as explained in [{ResistanceAuthority?.Citation}]: {Because}"
            : $"this rule gives it no Resistance to Fire damage [{Authority.Citation}]: {Because}";
}
