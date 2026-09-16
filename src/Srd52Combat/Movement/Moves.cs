using System.Collections.Immutable;
using RulesKernel.Provenance;
using Srd52Combat.Initiative;

namespace Srd52Combat.Movement;

/// <summary>
/// A mode of movement one move can be made of (<c>movement-modes</c>, "Combat / Movement and
/// Position / p. 14"): your regular movement, and the four the sentence names. What each of the four
/// costs is in the Rules Glossary (<c>movement-modes-glossary</c>), outside the extent. There is no
/// default: <c>default</c> is refused.
/// </summary>
public enum MovementMode
{
    /// <summary>Your regular movement, which the other modes combine with.</summary>
    Regular = 1,

    /// <summary>Climbing.</summary>
    Climbing = 2,

    /// <summary>Crawling.</summary>
    Crawling = 3,

    /// <summary>Jumping.</summary>
    Jumping = 4,

    /// <summary>Swimming.</summary>
    Swimming = 5,
}

/// <summary>
/// One move made of modes (<c>movement-modes</c>): "These different modes of movement can be combined
/// with your regular movement, or they can constitute your entire move." Every part of it draws on
/// the same Speed, and what each mode costs is outside the extent.
/// </summary>
/// <param name="Modes">The modes the move is made of, in the order the caller stated them.</param>
/// <param name="StatedBy">Who stated the move.</param>
/// <param name="Authority">The rule: <c>movement-modes</c>, "Combat / Movement and Position / p. 14".</param>
public sealed record CombinedMove(ImmutableArray<MovementMode> Modes, string StatedBy, SourceLocator Authority)
{
    /// <summary>Every part of the move draws on the one Speed: there is no second allowance for another mode.</summary>
    public bool DrawsOnOneSpeed => true;

    /// <summary>True when one mode constitutes the entire move.</summary>
    public bool EntireMoveInOneMode => Modes.Distinct().Count() == 1;

    /// <summary>True when a mode other than regular movement is combined with regular movement.</summary>
    public bool CombinedWithRegularMovement =>
        Modes.Contains(MovementMode.Regular) && Modes.Any(m => m != MovementMode.Regular);

    /// <inheritdoc/>
    public override string ToString() =>
        $"one move of {string.Join(", ", Modes)}, drawing on the one Speed, as stated by {StatedBy} [{Authority}]";
}

/// <summary>
/// Whether a creature can give itself the Prone condition on its turn (<c>dropping-prone</c>,
/// "Combat / Dropping Prone / p. 14"). What the condition then does is <c>prone-condition</c>,
/// outside the extent.
/// </summary>
/// <param name="Allowed">True when the creature can drop Prone: its Speed is not 0.</param>
/// <param name="SpeedInFeet">The Speed the caller stated, in feet.</param>
/// <param name="Authority">The rule: <c>dropping-prone</c>, "Combat / Dropping Prone / p. 14".</param>
public sealed record DroppingProneRuling(bool Allowed, int SpeedInFeet, SourceLocator Authority)
{
    /// <summary>Dropping Prone uses no action.</summary>
    public bool UsesAnAction => false;

    /// <summary>Dropping Prone uses none of your Speed.</summary>
    public int SpeedSpent => 0;

    /// <inheritdoc/>
    public override string ToString() =>
        Allowed
            ? $"you can give yourself the Prone condition, using no action and none of your Speed of {SpeedInFeet} feet [{Authority}]"
            : $"you can't give yourself the Prone condition: your Speed is {SpeedInFeet} [{Authority}]";
}

/// <summary>
/// Whether you may pass through another creature's space during your move
/// (<c>moving-through-creatures</c>, "Combat / Moving around Other Creatures / p. 14").
/// </summary>
/// <param name="MayPassThrough">True when the sentence names this creature's space as one you can pass through.</param>
/// <param name="Because">Which of the sentence's cases holds.</param>
/// <param name="YourSize">Your size category, as the caller stated it.</param>
/// <param name="Other">The creature whose space it is, as the caller stated it.</param>
/// <param name="Authority">The rule: <c>moving-through-creatures</c>, "Combat / Moving around Other Creatures / p. 14".</param>
public sealed record PassageRuling(
    bool MayPassThrough,
    string Because,
    CreatureSize YourSize,
    CreatureInSpace Other,
    SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        $"you ({YourSize}) can pass through {Other.Id}'s space: {Because} [{Authority}]";
}

/// <summary>
/// Whether another creature's space is Difficult Terrain for you
/// (<c>creature-space-difficult-terrain</c>, "Combat / Moving around Other Creatures / p. 14").
/// </summary>
/// <param name="DifficultTerrainForYou">True unless that creature is Tiny or your ally.</param>
/// <param name="Because">Which of the rule's cases holds.</param>
/// <param name="Other">The creature whose space it is, as the caller stated it.</param>
/// <param name="Authority">The rule: <c>creature-space-difficult-terrain</c>, "Combat / Moving around Other Creatures / p. 14".</param>
public sealed record CreatureSpaceTerrain(
    bool DifficultTerrainForYou,
    string Because,
    CreatureInSpace Other,
    SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        $"{Other.Id}'s space is {(DifficultTerrainForYou ? string.Empty : "not ")}Difficult Terrain for you: {Because} [{Authority}]";
}

/// <summary>
/// Whether a move may end where it ends (<c>no-willing-end-in-occupied-space</c>, "Combat / Moving
/// around Other Creatures / p. 14"): "You can't willingly end a move in a space occupied by another
/// creature."
/// </summary>
/// <param name="Allowed">True unless the move ends willingly in a space another creature occupies.</param>
/// <param name="Because">Which of the rule's cases holds.</param>
/// <param name="Willingly">Whether the move was the creature's own, as the caller stated it.</param>
/// <param name="Occupant">The creature occupying the space the move ends in, or null when none does.</param>
/// <param name="Authority">The rule: <c>no-willing-end-in-occupied-space</c>, "Combat / Moving around Other Creatures / p. 14".</param>
public sealed record EndOfMoveRuling(
    bool Allowed,
    string Because,
    bool Willingly,
    CreatureInSpace? Occupant,
    SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        $"the move {(Allowed ? "may" : "can't")} end there: {Because} [{Authority}]";
}

/// <summary>
/// Whether ending a turn in a space with another creature leaves you Prone
/// (<c>ending-turn-in-occupied-space</c>, "Combat / Moving around Other Creatures / p. 14"). What the
/// Prone condition then does is <c>prone-condition</c>, outside the extent.
/// </summary>
/// <param name="Prone">True when you have the Prone condition.</param>
/// <param name="Because">Which of the rule's cases holds.</param>
/// <param name="YourSize">Your size category, as the caller stated it.</param>
/// <param name="Other">The creature sharing the space at the end of the turn, or null when you ended the turn alone in it.</param>
/// <param name="Authority">The rule: <c>ending-turn-in-occupied-space</c>, "Combat / Moving around Other Creatures / p. 14".</param>
public sealed record EndOfTurnRuling(
    bool Prone,
    string Because,
    CreatureSize YourSize,
    CreatureInSpace? Other,
    SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        $"you ({YourSize}) {(Prone ? "have" : "do not have")} the Prone condition: {Because} [{Authority}]";
}

/// <summary>
/// How a turn ended, as the caller states it: whether another creature was in your space when the
/// turn ended. The prohibition is on ending a <em>move</em> there
/// (<c>no-willing-end-in-occupied-space</c>); the Prone consequence is on ending a <em>turn</em>
/// there, so a creature that shared a space mid-turn and left before the turn ended is not Prone.
/// </summary>
/// <param name="SharedAtTheEndOfTheTurn">True when another creature was in your space as the turn ended.</param>
/// <param name="Other">That creature; null when the caller states none shared the space.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record EndOfTurnStatement(bool SharedAtTheEndOfTheTurn, CreatureInSpace? Other, string StatedBy)
{
    /// <summary>The creature, checked to be present exactly when the space was shared.</summary>
    public CreatureInSpace? Other { get; } =
        SharedAtTheEndOfTheTurn == (Other is null)
            ? throw new ArgumentException(
                "a turn ended in a space with another creature names that creature, and one ended alone names none",
                nameof(Other))
            : Other;

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The turn ended with <paramref name="other"/> in your space.</summary>
    /// <param name="other">The creature sharing the space.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static EndOfTurnStatement SharedWith(CreatureInSpace other, string statedBy) =>
        new(SharedAtTheEndOfTheTurn: true, other, statedBy);

    /// <summary>The turn ended with nobody else in your space, whoever passed through it during the turn.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static EndOfTurnStatement SharedWithNobody(string statedBy) =>
        new(SharedAtTheEndOfTheTurn: false, null, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        SharedAtTheEndOfTheTurn
            ? $"the turn ended in a space with {Other!.Id}, as stated by {StatedBy}"
            : $"the turn ended in a space with no other creature, as stated by {StatedBy}";
}
