using System.Collections.Immutable;
using RulesKernel.Provenance;
using Srd52Combat.Initiative;

namespace Srd52Combat.Movement;

/// <summary>
/// One part of a move, as the caller states it: "you deduct the distance of each part of your move"
/// (<c>movement-deduction</c>, "Combat / Movement and Position / p. 14"). What the parts are, and how
/// far each goes, is the table's business; the engine is told them.
/// </summary>
/// <param name="Description">What the part is, in the caller's words.</param>
/// <param name="Feet">The distance of the part, in feet.</param>
public sealed record MovePart(string Description, int Feet)
{
    /// <summary>What the part is, checked to be non-empty.</summary>
    public string Description { get; } = Checks.Text(Description, nameof(Description));

    /// <summary>The distance, checked not to be negative.</summary>
    public int Feet { get; } = Feet >= 0
        ? Feet
        : throw new ArgumentOutOfRangeException(nameof(Feet), Feet, "a part of a move covers a distance of zero feet or more");

    /// <inheritdoc/>
    public override string ToString() => $"{Description} ({Feet} ft)";
}

/// <summary>
/// What <c>move-up-to-speed</c> says of a distance: "On your turn, you can move a distance equal to
/// your Speed or less. Or you can decide not to move", "Combat / Movement and Position / p. 14".
/// </summary>
/// <param name="Allowed">True when the distance is the Speed or less, moving nothing included.</param>
/// <param name="SpeedInFeet">The creature's Speed, in feet, as the caller states it.</param>
/// <param name="DistanceFeet">The distance asked about, in feet.</param>
/// <param name="Authority">The rule: "Combat / Movement and Position / p. 14".</param>
public sealed record MoveAllowance(bool Allowed, int SpeedInFeet, int DistanceFeet, SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        Allowed
            ? $"{DistanceFeet} ft is a distance equal to the Speed of {SpeedInFeet} ft or less, so it may be moved [{Authority.Citation}]"
            : $"{DistanceFeet} ft is more than the Speed of {SpeedInFeet} ft, so it may not be moved [{Authority.Citation}]";
}

/// <summary>
/// What <c>movement-deduction</c> makes of a move's parts: "However you're moving with your Speed,
/// you deduct the distance of each part of your move from it until it is used up or until you are
/// done moving, whichever comes first", "Combat / Movement and Position / p. 14".
/// </summary>
/// <param name="Taken">The parts deducted, in the order they were stated.</param>
/// <param name="NotTaken">The parts left: the first that the movement left could not cover, and everything after it.</param>
/// <param name="SpeedInFeet">The Speed the move draws on, in feet.</param>
/// <param name="MovementLeftFeet">What is left of the Speed once the taken parts are deducted.</param>
/// <param name="SpeedUsedUp">True when a part was left because the movement did not cover it.</param>
/// <param name="Authority">The rule: "Combat / Movement and Position / p. 14".</param>
public sealed record MovementSpent(
    ImmutableArray<MovePart> Taken,
    ImmutableArray<MovePart> NotTaken,
    int SpeedInFeet,
    int MovementLeftFeet,
    bool SpeedUsedUp,
    SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        SpeedUsedUp
            ? $"{string.Join(", ", Taken)} deducted from {SpeedInFeet} ft leaves {MovementLeftFeet} ft, which does not cover {NotTaken[0]}: "
              + $"the Speed is used up [{Authority.Citation}]"
            : $"{(Taken.IsEmpty ? "no part of a move" : string.Join(", ", Taken))} deducted from {SpeedInFeet} ft leaves {MovementLeftFeet} ft [{Authority.Citation}]";
}
