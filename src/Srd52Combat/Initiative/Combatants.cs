using System.Collections.Immutable;

namespace Srd52Combat.Initiative;

/// <summary>
/// What a combatant is, as the Initiative tie rule tells combatants apart ("Combat / Initiative /
/// p. 13"): monsters, player characters, and characters that are not player characters. Which one
/// a creature is, is a fact the caller states. There is no default: <c>default</c> is refused.
/// </summary>
public enum CombatantKind
{
    /// <summary>A monster: the GM rolls for it and decides ties among monsters.</summary>
    Monster = 1,

    /// <summary>A player character.</summary>
    PlayerCharacter = 2,

    /// <summary>A character that is not a player character.</summary>
    NonPlayerCharacter = 3,
}

/// <summary>
/// How a combatant's Initiative roll is made, as the caller states it, from every source: Surprise
/// ("Combat / Initiative / p. 13"), and the Incapacitated and Invisible conditions ("Rules Glossary /
/// p. 184", <c>incapacitated-condition</c> and <c>invisible-condition</c>, which map 2.0.0's
/// <c>initiative-roll</c> names in <c>dependsOn</c>). How Advantage and Disadvantage resolve
/// ("Playing the Game / Advantage/Disadvantage / p. 7") is outside this engine's slice, so a roll
/// stated to have either, or both, declines; the caller must still say which, because each changes
/// how many d20s are drawn. There is no default: <c>default</c> is refused.
/// </summary>
public enum D20Mode
{
    /// <summary>One d20, neither Advantage nor Disadvantage from any source.</summary>
    Straight = 1,

    /// <summary>The roll has Advantage (an Invisible combatant's does, p. 184).</summary>
    Advantage = 2,

    /// <summary>The roll has Disadvantage (a surprised or Incapacitated combatant's does, pp. 13 and 184).</summary>
    Disadvantage = 3,

    /// <summary>
    /// The roll has both Advantage and Disadvantage. Map 2.0.0's <c>draws</c> for <c>initiative-roll</c>
    /// counts one d20, because they cancel; that cancelling is <c>advantage-disadvantage</c>'s rule,
    /// <c>scope: out</c>, so the roll declines as the other two do (decision 0002).
    /// </summary>
    AdvantageAndDisadvantage = 4,
}

/// <summary>A participant in the combat, as the caller states it.</summary>
/// <remarks>
/// <paramref name="DexterityCheckModifier"/> is everything added to the d20 for this combatant's
/// Dexterity check. How it is made up (the ability modifier, a Proficiency Bonus if relevant,
/// circumstantial bonuses and penalties) is "Playing the Game / D20 Tests / p. 6", outside the
/// slice, so the engine takes the sum as a parameter and does not compute it.
/// </remarks>
/// <param name="Id">The combatant's name or handle; unique within one combat.</param>
/// <param name="Kind">Monster, player character, or another character.</param>
/// <param name="DexterityCheckModifier">The total modifier to the combatant's Dexterity check.</param>
/// <param name="Roll">Whether the roll has Advantage or Disadvantage.</param>
public sealed record Combatant(string Id, CombatantKind Kind, int DexterityCheckModifier, D20Mode Roll)
{
    /// <summary>The id, checked to be non-empty.</summary>
    public string Id { get; } = Checks.Text(Id, nameof(Id));

    /// <summary>The kind, checked to be stated.</summary>
    public CombatantKind Kind { get; } = Checks.Defined(Kind, nameof(Kind));

    /// <summary>The roll mode, checked to be stated.</summary>
    public D20Mode Roll { get; } = Checks.Defined(Roll, nameof(Roll));
}

/// <summary>A combatant's Initiative count: its Dexterity check total, however it was rolled.</summary>
/// <param name="CombatantId">The combatant.</param>
/// <param name="Kind">Monster, player character, or another character.</param>
/// <param name="Initiative">The check total ("Initiative count, or Initiative for short").</param>
public sealed record InitiativeCount(string CombatantId, CombatantKind Kind, int Initiative)
{
    /// <summary>The combatant, checked to be non-empty.</summary>
    public string CombatantId { get; } = Checks.Text(CombatantId, nameof(CombatantId));

    /// <summary>The kind, checked to be stated.</summary>
    public CombatantKind Kind { get; } = Checks.Defined(Kind, nameof(Kind));

    /// <inheritdoc/>
    public override string ToString() => $"{CombatantId} ({Kind}) {Initiative}";
}

/// <summary>
/// Whether the GM is using Initiative scores instead of rolling ("Rules Glossary / Initiative /
/// p. 184"), as the caller states it. That option is outside the slice and suspends
/// <c>initiative-roll</c> (rules-factory decision 0021), so the engine demands the statement,
/// records it, and never defaults it: defaulting to "rolling" draws dice a table did not throw,
/// and every later draw of a seeded combat moves with them.
/// </summary>
/// <param name="InUse">True when the GM has combatants use their Initiative scores.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record InitiativeScoreOptionStatement(bool InUse, string StatedBy)
{
    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The GM is using Initiative scores instead of rolling.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static InitiativeScoreOptionStatement ScoresInUse(string statedBy) => new(InUse: true, statedBy);

    /// <summary>Initiative is rolled.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static InitiativeScoreOptionStatement Rolling(string statedBy) => new(InUse: false, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        InUse
            ? $"the GM uses Initiative scores instead of rolling, as stated by {StatedBy}"
            : $"Initiative is rolled, not taken from Initiative scores, as stated by {StatedBy}";
}

/// <summary>
/// Which participants, if any, the caller states form a group of identical creatures for which the
/// GM makes a single Initiative roll. What makes such a group is <c>group-initiative</c>'s question,
/// which the map leaves unresolved, and it decides how many d20s are drawn. So the engine demands
/// the statement and never infers one in either direction.
/// </summary>
/// <param name="Groups">Each group's combatant ids; empty when the caller states there are none.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record IdenticalCreaturesStatement(ImmutableArray<ImmutableArray<string>> Groups, string StatedBy)
{
    /// <summary>The groups, checked to be present, each of at least two ids.</summary>
    public ImmutableArray<ImmutableArray<string>> Groups { get; } = CheckGroups(Groups);

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>No participants form a group of identical creatures.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static IdenticalCreaturesStatement None(string statedBy) => new([], statedBy);

    /// <summary>These participants form groups of identical creatures.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <param name="groups">Each group's combatant ids.</param>
    /// <returns>The statement.</returns>
    public static IdenticalCreaturesStatement Grouped(string statedBy, params string[][] groups) =>
        new([.. groups.Select(g => g.ToImmutableArray())], statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        Groups.IsEmpty
            ? $"no participants form a group of identical creatures, as stated by {StatedBy}"
            : $"{string.Join("; ", Groups.Select(g => string.Join(", ", g)))} form groups of identical creatures, as stated by {StatedBy}";

    private static ImmutableArray<ImmutableArray<string>> CheckGroups(ImmutableArray<ImmutableArray<string>> groups)
    {
        if (groups.IsDefault || groups.Any(g => g.IsDefault || g.Length < 2 || g.Any(string.IsNullOrWhiteSpace)))
        {
            throw new ArgumentException("every group of identical creatures names at least two combatants", nameof(Groups));
        }

        return groups;
    }
}

/// <summary>Argument checks shared by the Initiative types.</summary>
internal static class Checks
{
    internal static string Text(string text, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text, name);
        return text;
    }

    internal static T Defined<T>(T value, string name)
        where T : struct, Enum =>
        Enum.IsDefined(value) ? value : throw new ArgumentException($"{name} must be stated; {value} is not a {typeof(T).Name}", name);
}
