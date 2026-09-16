using System.Collections.Immutable;
using RulesKernel.Provenance;

namespace Srd52Combat.Attacks;

/// <summary>An attack is made, and by what: <c>attack-sources</c>, "Combat / Making an Attack / p. 14".</summary>
/// <param name="Source">What the attack is made from.</param>
/// <param name="Authority">The rule: "Combat / Making an Attack / p. 14".</param>
public sealed record AttackMade(AttackSource Source, SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() => $"an attack is made, taking the {Source} [{Authority.Citation}]";
}

/// <summary>
/// One of the three steps of an attack, in the order "Combat / Making an Attack / p. 15" gives
/// them. Each step is its own map entry, which the caller resolves in turn.
/// </summary>
/// <param name="Number">The step's number, 1 to 3.</param>
/// <param name="Name">The step's name, in the corpus's words.</param>
/// <param name="EntryId">The map entry that holds the step.</param>
public sealed record AttackStep(int Number, string Name, string EntryId)
{
    /// <inheritdoc/>
    public override string ToString() => $"{Number}: {Name} ({EntryId})";
}

/// <summary>The structure of an attack: <c>attack-structure</c>, "Combat / Making an Attack / p. 15".</summary>
/// <param name="Steps">The three steps, in order.</param>
/// <param name="Authority">The rule: "Combat / Making an Attack / p. 15".</param>
public sealed record AttackStructure(ImmutableArray<AttackStep> Steps, SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() => $"an attack has {Steps.Length} steps: {string.Join("; ", Steps)} [{Authority.Citation}]";
}

/// <summary>
/// Where a ranged attack's target stands against the attack's two ranges: <c>normal-and-long-range</c>,
/// "Combat / Range / p. 15".
/// </summary>
public enum RangeBand
{
    /// <summary>At or within the normal range: no Disadvantage from range.</summary>
    WithinNormalRange = 1,

    /// <summary>Beyond the normal range, at or within the long range: Disadvantage.</summary>
    BeyondNormalRange = 2,

    /// <summary>Beyond the long range: the target can't be attacked.</summary>
    BeyondLongRange = 3,
}

/// <summary>
/// What an attack's two ranges do to a shot at a given distance: <c>normal-and-long-range</c>,
/// "Combat / Range / p. 15".
/// </summary>
/// <param name="Band">Which of the three bands the distance falls in.</param>
/// <param name="CanAttack">False only beyond the long range: "you can't attack a target beyond long range".</param>
/// <param name="Effect">What the range does to the attack roll.</param>
/// <param name="Ranges">The attack's two ranges.</param>
/// <param name="DistanceFeet">The distance to the target, in feet.</param>
/// <param name="Authority">The rule: "Combat / Range / p. 15".</param>
public sealed record RangeVerdict(
    RangeBand Band,
    bool CanAttack,
    RollEffect Effect,
    TwoRanges Ranges,
    int DistanceFeet,
    SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        CanAttack
            ? $"{DistanceFeet} ft is {Band} ({Ranges}): the attack roll has {Effect} [{Authority.Citation}]"
            : $"{DistanceFeet} ft is {Band} ({Ranges}): the target can't be attacked [{Authority.Citation}]";
}

/// <summary>
/// The target an attack picks: <c>attack-target</c>, "Combat / Making an Attack / p. 15", step 1.
/// </summary>
/// <param name="Kind">A creature, an object, or a location.</param>
/// <param name="Target">The target's name or handle.</param>
/// <param name="WithinRange">Whether the target is within the attack's range.</param>
/// <param name="Range">What the range rule said.</param>
/// <param name="Authority">The rule: "Combat / Making an Attack / p. 15".</param>
public sealed record ChosenTarget(TargetKind Kind, string Target, bool WithinRange, RangeVerdict Range, SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        WithinRange
            ? $"the {Kind} {Target} is within the attack's range and is the target [{Authority.Citation}]; {Range}"
            : $"the {Kind} {Target} is not within the attack's range and can't be the target [{Authority.Citation}]; {Range}";
}

/// <summary>
/// One rule's determination about an attack roll: <c>unseen-attacker-advantage</c>,
/// <c>unseen-target-disadvantage</c> and <c>ranged-in-close-combat</c> each answer with one.
/// </summary>
/// <param name="Effect">What the rule gives the roll.</param>
/// <param name="EntryId">The map entry the determination is.</param>
/// <param name="Because">Why, in the engine's words, naming the statement it rests on.</param>
/// <param name="Authority">The rule's citation.</param>
public sealed record RollDetermination(RollEffect Effect, string EntryId, string Because, SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() => $"{Effect} ({EntryId}): {Because} [{Authority.Citation}]";
}

/// <summary>
/// What step 3 says about damage: <c>attack-resolution</c>, "Combat / Making an Attack / p. 15".
/// The attack roll is <c>attack-rolls</c>' and the damage roll <c>damage-rolls</c>', both outside
/// the slice, so the engine says whether damage is rolled and names the rule that rolls it.
/// </summary>
/// <param name="Hit">Whether the attack roll hit, as the caller stated it.</param>
/// <param name="DamageIsRolled">True on a hit, and only on a hit, unless the attack's own rules specify otherwise.</param>
/// <param name="Because">Why, in the engine's words.</param>
/// <param name="Authority">The rule: "Combat / Making an Attack / p. 15".</param>
/// <param name="DamageAuthority">Where the damage roll itself is made: <c>damage-rolls</c>, "Damage and Healing / Damage Rolls / p. 16"; null when no damage is rolled.</param>
public sealed record DamageOnHit(
    bool Hit,
    bool DamageIsRolled,
    string Because,
    SourceLocator Authority,
    SourceLocator? DamageAuthority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        DamageIsRolled
            ? $"damage is rolled [{DamageAuthority?.Citation}]: {Because} [{Authority.Citation}]"
            : $"no damage is rolled: {Because} [{Authority.Citation}]";
}

/// <summary>
/// What attacking a location does when the target is not in it: <c>wrong-location-misses</c>,
/// "Combat / Unseen Attackers and Targets / p. 14".
/// </summary>
/// <param name="Misses">True when the attack misses because the target is not in the targeted location.</param>
/// <param name="TargetedLocation">The location the attacker picked.</param>
/// <param name="Because">Why, in the engine's words.</param>
/// <param name="Authority">The rule: "Combat / Unseen Attackers and Targets / p. 14".</param>
public sealed record LocationAttack(bool Misses, string TargetedLocation, string Because, SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        Misses
            ? $"the attack on {TargetedLocation} misses: {Because} [{Authority.Citation}]"
            : $"this rule does not make the attack on {TargetedLocation} miss: {Because} [{Authority.Citation}]";
}

/// <summary>
/// What attacking while hidden does: <c>hidden-attacker-revealed</c>, "Combat / Unseen Attackers
/// and Targets / p. 14".
/// </summary>
/// <param name="LocationGivenAway">True when the attacker gives away its location.</param>
/// <param name="Because">Why, in the engine's words.</param>
/// <param name="Authority">The rule: "Combat / Unseen Attackers and Targets / p. 14".</param>
public sealed record HiddenAttacker(bool LocationGivenAway, string Because, SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        LocationGivenAway
            ? $"the attacker gives away its location: {Because} [{Authority.Citation}]"
            : $"the attacker gives nothing away: {Because} [{Authority.Citation}]";
}

/// <summary>
/// Whether leaving the attacker's reach provokes an Opportunity Attack:
/// <c>opportunity-attack-avoidance</c>, "Combat / Opportunity Attacks / p. 15".
/// </summary>
/// <param name="Provokes">False when this rule says the movement does not provoke.</param>
/// <param name="Because">Why, in the engine's words.</param>
/// <param name="Authority">The rule: "Combat / Opportunity Attacks / p. 15".</param>
public sealed record ProvocationVerdict(bool Provokes, string Because, SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        Provokes
            ? $"this rule does not avoid an Opportunity Attack: {Because} [{Authority.Citation}]"
            : $"no Opportunity Attack is provoked: {Because} [{Authority.Citation}]";
}

/// <summary>What one melee attack of an Opportunity Attack may be made with, "Combat / Opportunity Attacks / p. 15".</summary>
public enum MeleeAttackOption
{
    /// <summary>A melee attack with a weapon.</summary>
    Weapon = 1,

    /// <summary>An Unarmed Strike.</summary>
    UnarmedStrike = 2,
}

/// <summary>
/// Whether an Opportunity Attack can be made, and what it is: <c>opportunity-attack</c>,
/// "Combat / Opportunity Attacks / p. 15".
/// </summary>
/// <param name="CanBeMade">True when the attacker may make one.</param>
/// <param name="UsesReaction">True when making it takes the attacker's Reaction.</param>
/// <param name="Attacks">How many melee attacks it is: one.</param>
/// <param name="Options">What the melee attack may be made with.</param>
/// <param name="Timing">When it happens, in the corpus's words.</param>
/// <param name="Because">Why, in the engine's words.</param>
/// <param name="Authority">The rule: "Combat / Opportunity Attacks / p. 15".</param>
public sealed record OpportunityAttackOffer(
    bool CanBeMade,
    bool UsesReaction,
    int Attacks,
    ImmutableArray<MeleeAttackOption> Options,
    string Timing,
    string Because,
    SourceLocator Authority)
{
    /// <summary>When an Opportunity Attack happens, in the corpus's words.</summary>
    public const string RightBefore = "right before it leaves your reach";

    /// <summary>No Opportunity Attack can be made, for the reason given.</summary>
    /// <param name="because">Why, in the engine's words.</param>
    /// <param name="authority">The rule's citation.</param>
    /// <returns>The offer.</returns>
    public static OpportunityAttackOffer None(string because, SourceLocator authority) =>
        new(CanBeMade: false, UsesReaction: false, Attacks: 0, [], RightBefore, because, authority);

    /// <inheritdoc/>
    public override string ToString() =>
        CanBeMade
            ? $"an Opportunity Attack can be made: take a Reaction to make {Attacks} melee attack with {string.Join(" or ", Options)}, {Timing}: {Because} [{Authority.Citation}]"
            : $"no Opportunity Attack can be made: {Because} [{Authority.Citation}]";
}

/// <summary>
/// The modifiers step 2 determines: <c>attack-modifiers</c>, "Combat / Making an Attack / p. 15".
/// The Cover half of the step is <c>cover-degree</c>'s, which this engine has not built, so the
/// entry declines and no value of this type is produced yet; it is the type the step will answer
/// with once Cover can be determined.
/// </summary>
/// <param name="Cover">What the Cover determination said.</param>
/// <param name="Determinations">Every Advantage or Disadvantage the rules in this slice gave.</param>
/// <param name="Other">Penalties and bonuses from spells, special abilities and other effects, as the caller stated them.</param>
/// <param name="Authority">The rule: "Combat / Making an Attack / p. 15".</param>
public sealed record AttackModifiers(
    string Cover,
    ImmutableArray<RollDetermination> Determinations,
    ImmutableArray<string> Other,
    SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        $"Cover: {Cover}; {string.Join("; ", Determinations)}; other: {string.Join("; ", Other)} [{Authority.Citation}]";
}
