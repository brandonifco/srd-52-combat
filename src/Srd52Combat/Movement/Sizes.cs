using System.Collections.Immutable;
using RulesKernel.Provenance;
using Srd52Combat.Initiative;

namespace Srd52Combat.Movement;

/// <summary>
/// A creature's size category, "Combat / Creature Size / p. 14" (<c>size-categories</c>), in the
/// order the Creature Size and Space table lists them, "from smallest (Tiny) to largest
/// (Gargantuan)". Which category a creature belongs to is a fact the caller states; nothing in the
/// slice derives it. There is no default: <c>default</c> is refused.
/// </summary>
public enum CreatureSize
{
    /// <summary>Tiny, the smallest category.</summary>
    Tiny = 1,

    /// <summary>Small.</summary>
    Small = 2,

    /// <summary>Medium.</summary>
    Medium = 3,

    /// <summary>Large.</summary>
    Large = 4,

    /// <summary>Huge.</summary>
    Huge = 5,

    /// <summary>Gargantuan, the largest category.</summary>
    Gargantuan = 6,
}

/// <summary>
/// The size categories in order, the value <c>size-categories</c> states: the list the rules that
/// count steps along it read ("two sizes larger or smaller", "of a larger size than").
/// </summary>
/// <param name="FromSmallestToLargest">The categories, smallest first.</param>
/// <param name="Authority">The rule: <c>size-categories</c>, "Combat / Creature Size / p. 14".</param>
public sealed record SizeOrder(ImmutableArray<CreatureSize> FromSmallestToLargest, SourceLocator Authority)
{
    /// <summary>
    /// How many steps along the order separate <paramref name="from"/> and <paramref name="to"/>:
    /// positive when <paramref name="to"/> is the larger, negative when it is the smaller.
    /// </summary>
    /// <param name="from">One category.</param>
    /// <param name="to">The other.</param>
    /// <returns>The signed number of steps.</returns>
    public int Steps(CreatureSize from, CreatureSize to) =>
        FromSmallestToLargest.IndexOf(Checks.Defined(to, nameof(to)))
        - FromSmallestToLargest.IndexOf(Checks.Defined(from, nameof(from)));

    /// <summary>Whether <paramref name="size"/> is larger than <paramref name="other"/> along the order.</summary>
    /// <param name="size">The size compared.</param>
    /// <param name="other">The size compared with.</param>
    /// <returns>True when it is larger.</returns>
    public bool IsLarger(CreatureSize size, CreatureSize other) => Steps(other, size) > 0;

    /// <inheritdoc/>
    public override string ToString() =>
        $"{string.Join(", ", FromSmallestToLargest)} [{Authority}]";
}

/// <summary>
/// What the caller states about another creature sharing or blocking a space: who it is, its size
/// category, whether it is your ally, and whether it has the Incapacitated condition. Every one of
/// these is a fact the engine is given and never infers (rules-factory decision 0025): the slice
/// defines neither "ally" (<c>moving-through-creatures</c>'s note) nor the Incapacitated condition
/// (<c>incapacitated-condition</c>, outside the extent).
/// </summary>
/// <param name="Id">The creature's name or handle.</param>
/// <param name="Size">Its size category.</param>
/// <param name="IsYourAlly">True when the caller states it is your ally.</param>
/// <param name="HasIncapacitatedCondition">True when the caller states it has the Incapacitated condition.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record CreatureInSpace(
    string Id,
    CreatureSize Size,
    bool IsYourAlly,
    bool HasIncapacitatedCondition,
    string StatedBy)
{
    /// <summary>The id, checked to be non-empty.</summary>
    public string Id { get; } = Checks.Text(Id, nameof(Id));

    /// <summary>The size, checked to be stated.</summary>
    public CreatureSize Size { get; } = Checks.Defined(Size, nameof(Size));

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>A creature that is neither your ally nor Incapacitated, as the caller states it.</summary>
    /// <param name="id">The creature's name or handle.</param>
    /// <param name="size">Its size category.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static CreatureInSpace Stranger(string id, CreatureSize size, string statedBy) =>
        new(id, size, IsYourAlly: false, HasIncapacitatedCondition: false, statedBy);

    /// <summary>A creature the caller states is your ally.</summary>
    /// <param name="id">The creature's name or handle.</param>
    /// <param name="size">Its size category.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static CreatureInSpace Ally(string id, CreatureSize size, string statedBy) =>
        new(id, size, IsYourAlly: true, HasIncapacitatedCondition: false, statedBy);

    /// <summary>A creature the caller states has the Incapacitated condition.</summary>
    /// <param name="id">The creature's name or handle.</param>
    /// <param name="size">Its size category.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static CreatureInSpace Incapacitated(string id, CreatureSize size, string statedBy) =>
        new(id, size, IsYourAlly: false, HasIncapacitatedCondition: true, statedBy);

    /// <inheritdoc/>
    public override string ToString()
    {
        string[] facts =
        [
            $"{Size}",
            IsYourAlly ? "your ally" : "not your ally",
            HasIncapacitatedCondition ? "Incapacitated" : "not Incapacitated",
        ];
        return $"{Id} ({string.Join(", ", facts)}), as stated by {StatedBy}";
    }
}
