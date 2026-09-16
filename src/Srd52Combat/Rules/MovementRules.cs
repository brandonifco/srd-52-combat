using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Movement;

namespace Srd52Combat.Rules;

/// <summary>
/// Movement and space: the size categories (<c>size-categories</c>, "Combat / Creature Size /
/// p. 14"), the modes a move can be made of (<c>movement-modes</c>) and dropping Prone
/// (<c>dropping-prone</c>), and the three rules for moving around other creatures
/// (<c>moving-through-creatures</c>, <c>creature-space-difficult-terrain</c>,
/// <c>no-willing-end-in-occupied-space</c>) and ending a turn in one's space
/// (<c>ending-turn-in-occupied-space</c>).
/// </summary>
/// <remarks>
/// Five of these entries cite "Combat / Moving around Other Creatures / p. 14", so every decline
/// names its entry in <see cref="UnresolvedResult.Attempted"/>. What a creature's size is, whether it
/// is your ally, whether it has the Incapacitated condition, whether a move was willing and what a
/// square's terrain is are facts the caller states and the engine never infers.
/// </remarks>
public static class MovementRules
{
    /// <summary>
    /// <c>size-categories</c>: the categories in the order the Creature Size and Space table lists
    /// them, "from smallest (Tiny) to largest (Gargantuan)". The order is what the rules that count
    /// steps along it read.
    /// </summary>
    /// <returns>The order.</returns>
    public static SizeOrder Sizes() => Order;

    /// <summary>
    /// <c>movement-modes</c>: "Your movement can include climbing, crawling, jumping, and swimming
    /// … These different modes of movement can be combined with your regular movement, or they can
    /// constitute your entire move."
    /// </summary>
    /// <remarks>
    /// In the slice the rule says two things and no more: one move may mix these modes, and all of
    /// them draw on the same Speed. What each mode costs is the Rules Glossary's
    /// (<c>movement-modes-glossary</c>), outside the extent, so a caller asking this rule for a
    /// mode's cost is declined <see cref="UnresolvedReason.OutsideCurrentScope"/> citing it.
    /// </remarks>
    /// <param name="modes">The modes the move is made of, in order, as the caller states them.</param>
    /// <param name="statedBy">Who stated the move.</param>
    /// <param name="askingWhatAModeCosts">A mode whose cost is asked for; null when the caller asks only whether the move is one move.</param>
    /// <returns>The move, or the decline citing <c>movement-modes-glossary</c>.</returns>
    /// <exception cref="ArgumentException"><paramref name="modes"/> is empty or names a mode that is not stated.</exception>
    public static Resolution<CombinedMove> Combine(
        IReadOnlyList<MovementMode> modes,
        string statedBy,
        MovementMode? askingWhatAModeCosts = null)
    {
        ArgumentNullException.ThrowIfNull(modes);
        ArgumentException.ThrowIfNullOrWhiteSpace(statedBy);
        if (modes.Count == 0)
        {
            throw new ArgumentException("a move is made of at least one mode of movement", nameof(modes));
        }

        foreach (var mode in modes)
        {
            Checks.Defined(mode, nameof(modes));
        }

        if (askingWhatAModeCosts is { } asked)
        {
            Checks.Defined(asked, nameof(askingWhatAModeCosts));
            return Resolution<CombinedMove>.FromUnresolved(new UnresolvedResult(
                UnresolvedReason.OutsideCurrentScope,
                $"{Cited.Attempting(MapEntries.MovementModes)} for what {asked} costs: each mode is explained in the Rules "
                + $"Glossary ('{MapEntries.MovementModesGlossary.Id}'), outside this engine's extent",
                MapEntries.MovementModesGlossary.Locator));
        }

        return Resolution<CombinedMove>.FromValue(
            new CombinedMove([.. modes], statedBy, MapEntries.MovementModes.Locator));
    }

    /// <summary>
    /// <c>dropping-prone</c>: "On your turn, you can give yourself the Prone condition … without
    /// using an action or any of your Speed, but you can't do so if your Speed is 0."
    /// </summary>
    /// <param name="speedInFeet">The creature's Speed in feet, as the caller states it.</param>
    /// <returns>Whether it can drop Prone, and at what cost.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="speedInFeet"/> is negative.</exception>
    public static DroppingProneRuling DropProne(int speedInFeet)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(speedInFeet);
        return new DroppingProneRuling(speedInFeet != 0, speedInFeet, MapEntries.DroppingProne.Locator);
    }

    /// <summary>
    /// <c>moving-through-creatures</c>: "During your move, you can pass through the space of an ally,
    /// a creature that has the Incapacitated condition …, a Tiny creature, or a creature that is two
    /// sizes larger or smaller than you."
    /// </summary>
    /// <remarks>
    /// The four cases the sentence names resolve on the corpus's own words, and name no ruling.
    /// <para>
    /// The entry's question has two parts, and the owner has ruled on one. **Brandon ruled on
    /// 2026-09-16 that "two sizes larger or smaller" means two or more**
    /// (<c>moving-through-creatures/two-or-more</c>, <c>docs/decisions/0007</c>), so a creature more
    /// than two sizes larger or smaller may be passed through — a Medium creature passes through a
    /// Huge or a Gargantuan one — and that answer names the ruling in
    /// <see cref="PassageRuling.Rulings"/>.
    /// </para>
    /// <para>
    /// The other part is still declined: the sentence permits passage through four kinds of space
    /// without saying that every other creature's space is impassable. So a creature fewer than two
    /// sizes away that is neither Tiny nor an ally nor Incapacitated still declines
    /// <see cref="UnresolvedReason.RequiresInterpretation"/> citing this entry, and the engine chooses
    /// no reading of it. The overlay's <c>declines</c> names that part and its test.
    /// </para>
    /// </remarks>
    /// <param name="yourSize">Your size category, as the caller states it.</param>
    /// <param name="other">The creature whose space it is, as the caller states it.</param>
    /// <returns>That you may pass through, or the decline of the part nobody has ruled on.</returns>
    public static Resolution<PassageRuling> PassThrough(CreatureSize yourSize, CreatureInSpace other)
    {
        ArgumentNullException.ThrowIfNull(other);
        Checks.Defined(yourSize, nameof(yourSize));

        int steps = Math.Abs(Order.Steps(yourSize, other.Size));
        string? because =
            other.IsYourAlly ? "it is your ally"
            : other.HasIncapacitatedCondition ? "it has the Incapacitated condition"
            : other.Size == CreatureSize.Tiny ? "it is Tiny"
            : steps == TwoSizes ? "it is two sizes larger or smaller than you"
            : null;
        if (because is not null)
        {
            return Resolution<PassageRuling>.FromValue(new PassageRuling(
                true, because, yourSize, other, MapEntries.MovingThroughCreatures.Locator, OwnerRulings.None));
        }

        if (steps > TwoSizes)
        {
            // The owner's ruling, not the corpus's words: "two sizes larger or smaller" is two or more.
            return Resolution<PassageRuling>.FromValue(new PassageRuling(
                true,
                $"it is {steps} sizes {(Order.IsLarger(other.Size, yourSize) ? "larger" : "smaller")} than you, and "
                + "“two sizes larger or smaller” means two or more",
                yourSize,
                other,
                MapEntries.MovingThroughCreatures.Locator,
                [OwnerRulings.TwoSizesMeansTwoOrMore]));
        }

        return Resolution<PassageRuling>.FromUnresolved(new UnresolvedResult(
            UnresolvedReason.RequiresInterpretation,
            $"{Cited.Attempting(MapEntries.MovingThroughCreatures)} for you ({yourSize}) and {other}: {other.Id} is none of "
            + "the four kinds of creature the sentence names, and the sentence does not say that every other creature's space "
            + "cannot be passed through",
            MapEntries.MovingThroughCreatures.Locator));
    }

    /// <summary>
    /// <c>creature-space-difficult-terrain</c>: "Another creature's space is Difficult Terrain for
    /// you unless that creature is Tiny or your ally."
    /// </summary>
    /// <param name="other">The creature whose space it is, as the caller states it.</param>
    /// <returns>Whether its space is Difficult Terrain for you.</returns>
    public static CreatureSpaceTerrain SpaceOf(CreatureInSpace other)
    {
        ArgumentNullException.ThrowIfNull(other);
        string? exempt =
            other.Size == CreatureSize.Tiny ? "it is Tiny"
            : other.IsYourAlly ? "it is your ally"
            : null;
        return new CreatureSpaceTerrain(
            exempt is null,
            exempt ?? "it is another creature, and neither Tiny nor your ally",
            other,
            MapEntries.CreatureSpaceDifficultTerrain.Locator);
    }

    /// <summary>
    /// <c>no-willing-end-in-occupied-space</c>: "You can't willingly end a move in a space occupied
    /// by another creature."
    /// </summary>
    /// <remarks>
    /// The rule forbids one thing: ending a move there willingly. It does not matter whose space it
    /// is -- an ally's space is as forbidden as a stranger's -- and a move the creature did not
    /// choose is not forbidden by this rule. Whether the move was willing is the caller's statement.
    /// </remarks>
    /// <param name="occupant">The creature occupying the space the move would end in; null when the caller states none does.</param>
    /// <param name="willingly">Whether the creature chose to end its move there, as the caller states it.</param>
    /// <returns>Whether the move may end there.</returns>
    public static EndOfMoveRuling EndMove(CreatureInSpace? occupant, bool willingly)
    {
        string because =
            occupant is null ? "no other creature occupies the space"
            : !willingly ? $"the move was not the creature's own, and the rule forbids only ending one willingly in {occupant.Id}'s space"
            : $"the space is occupied by {occupant.Id}";
        return new EndOfMoveRuling(
            occupant is null || !willingly,
            because,
            willingly,
            occupant,
            MapEntries.NoWillingEndInOccupiedSpace.Locator);
    }

    /// <summary>
    /// <c>ending-turn-in-occupied-space</c>: "If you somehow end a turn in a space with another
    /// creature, you have the Prone condition … unless you are Tiny or are of a larger size than the
    /// other creature."
    /// </summary>
    /// <remarks>
    /// The consequence is on ending a <em>turn</em> in the space, not on ending a move there
    /// (<c>no-willing-end-in-occupied-space</c>), so a creature that shared a space during the turn
    /// and left before it ended is not Prone. "Of a larger size" counts one step along
    /// <c>size-categories</c>: one size larger is enough, and the same size is not.
    /// </remarks>
    /// <param name="yourSize">Your size category, as the caller states it.</param>
    /// <param name="turn">How the turn ended, as the caller states it.</param>
    /// <returns>Whether you have the Prone condition.</returns>
    public static EndOfTurnRuling EndTurn(CreatureSize yourSize, EndOfTurnStatement turn)
    {
        ArgumentNullException.ThrowIfNull(turn);
        Checks.Defined(yourSize, nameof(yourSize));
        if (turn.Other is not { } other)
        {
            return new EndOfTurnRuling(
                false,
                "the turn did not end in a space with another creature",
                yourSize,
                null,
                MapEntries.EndingTurnInOccupiedSpace.Locator);
        }

        string? exempt =
            yourSize == CreatureSize.Tiny ? "you are Tiny"
            : Order.IsLarger(yourSize, other.Size) ? $"you are of a larger size than {other.Id} ({other.Size})"
            : null;
        return new EndOfTurnRuling(
            exempt is null,
            exempt ?? $"the turn ended in a space with {other.Id} ({other.Size}), and you are neither Tiny nor of a larger size",
            yourSize,
            other,
            MapEntries.EndingTurnInOccupiedSpace.Locator);
    }

    /// <summary>Two sizes: the distance "two sizes larger or smaller than you" names.</summary>
    private const int TwoSizes = 2;

    /// <summary>
    /// The size categories in the table's row order, smallest first, which <c>size-categories</c>
    /// states runs "from smallest (Tiny) to largest (Gargantuan)".
    /// </summary>
    private static readonly SizeOrder Order = new(
        [CreatureSize.Tiny, CreatureSize.Small, CreatureSize.Medium, CreatureSize.Large, CreatureSize.Huge, CreatureSize.Gargantuan],
        MapEntries.SizeCategories.Locator);
}
