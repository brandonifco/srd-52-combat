using System.Collections.Immutable;
using Srd52Combat.Movement;
using Srd52Combat.Turn;

namespace Srd52Combat.Rules;

/// <summary>
/// What a move may cost, "Combat / Movement and Position / p. 14": how far a creature may move
/// (<c>move-up-to-speed</c>) and how a move's parts are deducted from its Speed
/// (<c>movement-deduction</c>).
/// </summary>
/// <remarks>
/// <para>
/// Speed is not this slice's: "A character's Speed is determined during character creation. A
/// monster's Speed is noted in the monster's stat block" (<c>speed-and-size-sources</c>,
/// <c>scope: out</c>), so it reaches the engine as a parameter. These two rules are what
/// <c>mounting-cost</c> spends: mounting costs an amount of movement, and this is the budget it
/// comes out of.
/// </para>
/// <para>
/// <c>move-up-to-speed</c> "restates <c>turn-move-and-action</c>'s movement half, in agreement"
/// (the entry's own note), and the turn entry was built first (MAP-FINDINGS finding 16). So this
/// rule answers <em>through</em> <see cref="TurnRules.Take"/> rather than counting the Speed again,
/// and <see cref="Deduct"/> asks it of each part in turn: one sentence, one implementation.
/// </para>
/// </remarks>
public static class MovementBudgetRules
{
    /// <summary>
    /// <c>move-up-to-speed</c>: "On your turn, you can move a distance equal to your Speed or less.
    /// Or you can decide not to move." "Or less" includes nothing at all, and the rule permits
    /// exactly the distances from zero to the Speed.
    /// </summary>
    /// <param name="speedInFeet">The creature's Speed in feet, as the caller states it.</param>
    /// <param name="distanceFeet">The distance asked about, in feet.</param>
    /// <param name="statedBy">Who is answerable for the statement of the move.</param>
    /// <returns>Whether the distance may be moved.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A figure is negative.</exception>
    public static MoveAllowance UpToSpeed(int speedInFeet, int distanceFeet, string statedBy)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(speedInFeet);
        ArgumentOutOfRangeException.ThrowIfNegative(distanceFeet);
        ArgumentException.ThrowIfNullOrWhiteSpace(statedBy);
        bool allowed = TurnRules.Take(speedInFeet, [TurnStep.Moves(distanceFeet)], statedBy)
            .Match(turn => turn.WithinTurn, _ => false);
        return new MoveAllowance(allowed, speedInFeet, distanceFeet, MapEntries.MoveUpToSpeed.Locator);
    }

    /// <summary>
    /// <c>movement-deduction</c>: "However you're moving with your Speed, you deduct the distance of
    /// each part of your move from it until it is used up or until you are done moving, whichever
    /// comes first."
    /// </summary>
    /// <remarks>
    /// Each part is deducted whole, in the order the caller states the parts. A part the movement
    /// left does not cover is not taken, and neither is anything after it: the Speed is used up
    /// first, and <c>move-up-to-speed</c> allows "a distance equal to your Speed or less", so no
    /// rule of the slice takes the total past the Speed. The <em>rest</em> of such a part is not
    /// moved in halves here: what the corpus deducts is "the distance of each part of your move",
    /// and a part the caller states is one distance.
    /// </remarks>
    /// <param name="speedInFeet">The creature's Speed in feet, as the caller states it.</param>
    /// <param name="parts">The parts of the move, in order, as the caller states them.</param>
    /// <param name="statedBy">Who is answerable for the statement of the move.</param>
    /// <returns>What was deducted and what is left.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="speedInFeet"/> is negative.</exception>
    /// <exception cref="ArgumentException"><paramref name="parts"/> contains a null.</exception>
    public static MovementSpent Deduct(int speedInFeet, IReadOnlyList<MovePart> parts, string statedBy)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(speedInFeet);
        ArgumentNullException.ThrowIfNull(parts);
        ArgumentException.ThrowIfNullOrWhiteSpace(statedBy);
        if (parts.Any(p => p is null))
        {
            throw new ArgumentException("no part of the move is null", nameof(parts));
        }

        var taken = ImmutableArray.CreateBuilder<MovePart>();
        var notTaken = ImmutableArray.CreateBuilder<MovePart>();
        int used = 0;
        bool usedUp = false;
        foreach (var part in parts)
        {
            // What the move so far may be is move-up-to-speed's, which is turn-move-and-action's.
            if (usedUp || !UpToSpeed(speedInFeet, used + part.Feet, statedBy).Allowed)
            {
                usedUp = true;
                notTaken.Add(part);
                continue;
            }

            used += part.Feet;
            taken.Add(part);
        }

        int left = speedInFeet - used;

        return new MovementSpent(
            taken.ToImmutable(),
            notTaken.ToImmutable(),
            speedInFeet,
            left,
            usedUp,
            MapEntries.MovementDeduction.Locator);
    }
}
