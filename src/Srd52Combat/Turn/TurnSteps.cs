using System.Collections.Immutable;
using RulesKernel.Provenance;
using Srd52Combat.Initiative;

namespace Srd52Combat.Turn;

/// <summary>
/// What a creature does on its turn, as the caller states it. The turn rules count two things:
/// the distance moved (<c>turn-move-and-action</c>, "Combat / Your Turn / p. 13") and the action
/// taken. A Bonus Action and a Reaction are steps a move can be broken around
/// (<c>break-up-move</c>, "Combat / Breaking Up Your Move / p. 14"); whether the creature has one
/// to take is <c>bonus-actions</c> and <c>reactions</c> (p. 10), outside this engine's slice, so
/// the caller states that it took one and the engine counts neither against the turn's budget.
/// There is no default: <c>default</c> is refused.
/// </summary>
public enum TurnStepKind
{
    /// <summary>Part of the creature's move, in feet.</summary>
    Move = 1,

    /// <summary>The one action a turn allows.</summary>
    Action = 2,

    /// <summary>A Bonus Action (<c>bonus-actions</c>, p. 10, outside the slice).</summary>
    BonusAction = 3,

    /// <summary>A Reaction (<c>reactions</c>, p. 10, outside the slice).</summary>
    Reaction = 4,
}

/// <summary>
/// One step of a turn, in the order the creature takes it: a part of its move, or an action, Bonus
/// Action or Reaction. What the actions are is <c>actions-table</c> ("Playing the Game / Actions /
/// p. 9"), outside the slice, so an action is named and never interpreted.
/// </summary>
/// <param name="Kind">Move, action, Bonus Action or Reaction.</param>
/// <param name="Feet">The distance of this part of the move; 0 for every other kind.</param>
/// <param name="What">What was done, in the caller's words.</param>
public sealed record TurnStep(TurnStepKind Kind, int Feet, string What)
{
    /// <summary>The kind, checked to be stated.</summary>
    public TurnStepKind Kind { get; } = Checks.Defined(Kind, nameof(Kind));

    /// <summary>The distance, checked to be a distance this kind of step can have.</summary>
    public int Feet { get; } = CheckFeet(Kind, Feet);

    /// <summary>What was done, checked to be non-empty.</summary>
    public string What { get; } = Checks.Text(What, nameof(What));

    /// <summary>Part of the creature's move.</summary>
    /// <param name="feet">The distance of this part, in feet.</param>
    /// <returns>The step.</returns>
    public static TurnStep Moves(int feet) => new(TurnStepKind.Move, feet, $"a move of {feet} feet");

    /// <summary>The turn's action.</summary>
    /// <param name="what">The action, in the caller's words.</param>
    /// <returns>The step.</returns>
    public static TurnStep Acts(string what) => new(TurnStepKind.Action, 0, what);

    /// <summary>A Bonus Action the caller states the creature takes.</summary>
    /// <param name="what">The Bonus Action, in the caller's words.</param>
    /// <returns>The step.</returns>
    public static TurnStep BonusAction(string what) => new(TurnStepKind.BonusAction, 0, what);

    /// <summary>A Reaction the caller states the creature takes.</summary>
    /// <param name="what">The Reaction, in the caller's words.</param>
    /// <returns>The step.</returns>
    public static TurnStep Reaction(string what) => new(TurnStepKind.Reaction, 0, what);

    /// <inheritdoc/>
    public override string ToString() => Kind == TurnStepKind.Move ? $"moves {Feet} feet" : $"takes {What} ({Kind})";

    private static int CheckFeet(TurnStepKind kind, int feet)
    {
        if (feet < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Feet), feet, "a part of a move is a distance, and a distance is not negative");
        }

        return kind == TurnStepKind.Move || feet == 0
            ? feet
            : throw new ArgumentException($"a {kind} step covers no distance; its Feet must be 0", nameof(Feet));
    }
}

/// <summary>
/// A turn as the creature took it: "On your turn, you can move a distance up to your Speed and take
/// one action. You decide whether to move first or take your action first"
/// (<c>turn-move-and-action</c>, "Combat / Your Turn / p. 13").
/// </summary>
/// <param name="Speed">The creature's Speed, as the caller states it (<c>speed-and-size-sources</c>, outside the slice).</param>
/// <param name="Steps">The steps, in the order they were taken.</param>
/// <param name="MovementUsed">The distance moved in total, however the move was split.</param>
/// <param name="Actions">How many actions were taken.</param>
/// <param name="Exceeded">What the turn allows and these steps went past; empty when the turn is within the budget.</param>
/// <param name="StatedBy">Who is answerable for the statement of the turn.</param>
/// <param name="Authority">The rule: <c>turn-move-and-action</c>, "Combat / Your Turn / p. 13".</param>
public sealed record TakenTurn(
    int Speed,
    ImmutableArray<TurnStep> Steps,
    int MovementUsed,
    int Actions,
    ImmutableArray<string> Exceeded,
    string StatedBy,
    SourceLocator Authority)
{
    /// <summary>The Speed left over; 0 once the whole Speed is used up.</summary>
    public int MovementRemaining => Math.Max(0, Speed - MovementUsed);

    /// <summary>Whether the turn's one action was taken.</summary>
    public bool ActionTaken => Actions > 0;

    /// <summary>Whether the steps stay inside what the turn allows.</summary>
    public bool WithinTurn => Exceeded.IsEmpty;

    /// <summary>What the creature did first, which the rule leaves to it; null on a turn with no steps.</summary>
    public TurnStepKind? Started => Steps.IsEmpty ? null : Steps[0].Kind;

    /// <inheritdoc/>
    public override string ToString() =>
        $"{MovementUsed} of {Speed} feet moved, {Actions} action(s) taken"
        + (WithinTurn ? string.Empty : $"; beyond the turn: {string.Join("; ", Exceeded)}");
}

/// <summary>One part of a broken-up move: what was moved, and what is left of the Speed after it.</summary>
/// <param name="Feet">The distance of this part.</param>
/// <param name="After">What the part follows (an action, Bonus Action or Reaction), or null for the part that opens the turn.</param>
/// <param name="RemainingAfter">The movement left after this part: the remainder of the Speed, never a fresh one.</param>
public sealed record MoveSegment(int Feet, string? After, int RemainingAfter)
{
    /// <inheritdoc/>
    public override string ToString() =>
        (After is null ? $"{Feet} feet" : $"{Feet} feet after {After}") + $", {RemainingAfter} feet left";
}

/// <summary>
/// A move broken up around the turn's other steps: "You can break up your move, using some of its
/// movement before and after any action, Bonus Action, or Reaction you take on the same turn"
/// (<c>break-up-move</c>, "Combat / Breaking Up Your Move / p. 14").
/// </summary>
/// <param name="Segments">Each part of the move, in order, with the movement left after it.</param>
/// <param name="Turn">The turn the move was made on, and its budget.</param>
/// <param name="Authority">The rule: <c>break-up-move</c>, "Combat / Breaking Up Your Move / p. 14".</param>
public sealed record BrokenMove(ImmutableArray<MoveSegment> Segments, TakenTurn Turn, SourceLocator Authority)
{
    /// <summary>The creature's Speed.</summary>
    public int Speed => Turn.Speed;

    /// <summary>The distance moved in total, across every part.</summary>
    public int MovementUsed => Turn.MovementUsed;

    /// <summary>The movement left when the turn ended.</summary>
    public int MovementRemaining => Turn.MovementRemaining;

    /// <summary>Whether the parts together stay inside what the turn allows.</summary>
    public bool WithinTurn => Turn.WithinTurn;

    /// <summary>What the turn allows and these parts went past; empty when the move is within the budget.</summary>
    public ImmutableArray<string> Exceeded => Turn.Exceeded;

    /// <inheritdoc/>
    public override string ToString() => string.Join("; ", Segments);
}

/// <summary>
/// What a turn forgoes: "You can forgo moving, taking an action, or doing anything at all on your
/// turn" (<c>doing-nothing</c>, "Combat / Your Turn / p. 14"). Whatever is forgone, the turn is
/// permitted.
/// </summary>
/// <param name="Turn">The turn, and what little of it was used.</param>
/// <param name="Movement">True when the creature moved no distance at all.</param>
/// <param name="Action">True when the creature took no action.</param>
/// <param name="Authority">The rule: <c>doing-nothing</c>, "Combat / Your Turn / p. 14".</param>
public sealed record ForgoneTurn(TakenTurn Turn, bool Movement, bool Action, SourceLocator Authority)
{
    /// <summary>True when the creature did nothing at all on its turn.</summary>
    public bool Everything => Turn.Steps.IsEmpty;

    /// <summary>Forgoing any of it is permitted: the rule says the creature can.</summary>
    public bool Permitted => true;

    /// <inheritdoc/>
    public override string ToString() => Everything
        ? "forgoes doing anything at all"
        : Movement || Action
            ? $"forgoes {string.Join(" and ", new[] { Movement ? "moving" : null, Action ? "taking an action" : null }.Where(w => w is not null))}"
            : "forgoes nothing";
}
