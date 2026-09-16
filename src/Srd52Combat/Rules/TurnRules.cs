using System.Collections.Immutable;
using RulesKernel.Resolution;
using Srd52Combat.Turn;

namespace Srd52Combat.Rules;

/// <summary>
/// Your turn: the budget of a turn (<c>turn-move-and-action</c>, "Combat / Your Turn / p. 13"), a
/// move broken up around the turn's other steps (<c>break-up-move</c>, "Combat / Breaking Up Your
/// Move / p. 14"), forgoing any of it (<c>doing-nothing</c>, p. 14), the one free object
/// interaction (<c>free-object-interaction</c>, p. 13), what communicating costs
/// (<c>communication-cost</c>, p. 13), and the GM's requirement that governs both of those
/// (<c>gm-requires-action</c>, p. 14).
/// </summary>
public static class TurnRules
{
    /// <summary>
    /// The turn's budget: "On your turn, you can move a distance up to your Speed and take one
    /// action. You decide whether to move first or take your action first." The steps are the
    /// caller's, in the order it took them; the rule counts the distance and the actions, and names
    /// what the turn does not allow. What an action is, is <c>actions-table</c> (p. 9), outside the
    /// slice: an action is counted, never interpreted. A Bonus Action or a Reaction costs neither
    /// the move nor the action here; that a creature has one to take is <c>bonus-actions</c> and
    /// <c>reactions</c> (p. 10), also outside the slice.
    /// </summary>
    /// <param name="speed">The creature's Speed, as the caller states it.</param>
    /// <param name="steps">The turn's steps, in the order taken.</param>
    /// <param name="statedBy">Who is answerable for the statement of the turn.</param>
    /// <returns>The turn as taken, and what (if anything) it went past.</returns>
    /// <exception cref="ArgumentException">A step is null, or the Speed is negative.</exception>
    public static Resolution<TakenTurn> Take(int speed, IReadOnlyList<TurnStep> steps, string statedBy)
    {
        var taken = Taken(speed, steps, statedBy);
        return Resolution<TakenTurn>.FromValue(taken);
    }

    /// <summary>
    /// A move broken up: "You can break up your move, using some of its movement before and after
    /// any action, Bonus Action, or Reaction you take on the same turn." Each part is deducted from
    /// the same Speed, so what is left after an action is the remainder and never a fresh Speed.
    /// </summary>
    /// <param name="speed">The creature's Speed, as the caller states it.</param>
    /// <param name="steps">The turn's steps, in the order taken.</param>
    /// <param name="statedBy">Who is answerable for the statement of the turn.</param>
    /// <returns>Each part of the move, what it follows, and the movement left after it.</returns>
    /// <exception cref="ArgumentException">A step is null, or the Speed is negative.</exception>
    public static Resolution<BrokenMove> BreakUp(int speed, IReadOnlyList<TurnStep> steps, string statedBy)
    {
        var taken = Taken(speed, steps, statedBy);
        var segments = ImmutableArray.CreateBuilder<MoveSegment>();
        int used = 0;
        string? after = null;
        foreach (var step in taken.Steps)
        {
            if (step.Kind == TurnStepKind.Move)
            {
                used += step.Feet;
                segments.Add(new MoveSegment(step.Feet, after, Math.Max(0, speed - used)));
                after = null;
            }
            else
            {
                after = step.What;
            }
        }

        return Resolution<BrokenMove>.FromValue(new BrokenMove(segments.ToImmutable(), taken, MapEntries.BreakUpMove.Locator));
    }

    /// <summary>
    /// Forgoing the turn: "You can forgo moving, taking an action, or doing anything at all on your
    /// turn." Whatever the creature leaves undone, the turn is permitted, and nothing is required of
    /// it. The advice that follows in the corpus ("consider taking the defensive Dodge action or the
    /// Ready action to delay acting") states no rule and creates no delay, so the engine has none.
    /// </summary>
    /// <param name="speed">The creature's Speed, as the caller states it.</param>
    /// <param name="steps">The turn's steps, in the order taken; empty when the creature does nothing at all.</param>
    /// <param name="statedBy">Who is answerable for the statement of the turn.</param>
    /// <returns>What the turn forgoes, and that forgoing it is permitted.</returns>
    /// <exception cref="ArgumentException">A step is null, or the Speed is negative.</exception>
    public static Resolution<ForgoneTurn> Forgo(int speed, IReadOnlyList<TurnStep> steps, string statedBy)
    {
        var taken = Taken(speed, steps, statedBy);
        return Resolution<ForgoneTurn>.FromValue(
            new ForgoneTurn(taken, taken.MovementUsed == 0, !taken.ActionTaken, MapEntries.DoingNothing.Locator));
    }

    /// <summary>
    /// The GM's requirement, an assertion (row 8): "The GM might require you to use an action for
    /// any of these activities when it needs special care or when it presents an unusual obstacle."
    /// The measure is the GM's own, and the engine never applies it; it takes the determination as
    /// stated, checks the party against the map's <c>assertedBy</c>, and records it.
    /// </summary>
    /// <param name="requirement">The GM's statement.</param>
    /// <returns>The statement, as the rule reads it.</returns>
    public static Resolution<GmActionRequirement> Requires(GmActionRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(requirement);
        return Resolution<GmActionRequirement>.FromValue(requirement);
    }

    /// <summary>
    /// Interacting with things: "You can interact with one object or feature of the environment for
    /// free, during either your move or action… If you want to interact with a second object, you
    /// need to take the Utilize action." An interaction the GM requires an action for
    /// (<c>gm-requires-action</c>, the gate in <c>suspendedBy</c>) costs an action and is not the
    /// turn's free one, and neither is one the object's own description always requires an action
    /// for; the free interaction is the first of the rest.
    /// </summary>
    /// <param name="interactions">The turn's interactions, in the order they happen.</param>
    /// <param name="requirement">The GM's requirement, the assertion <c>gm-requires-action</c>; never defaulted.</param>
    /// <returns>Each interaction and what it costs.</returns>
    /// <exception cref="ArgumentException">An interaction is null.</exception>
    public static Resolution<TurnInteractions> Interactions(IReadOnlyList<ObjectInteraction> interactions, GmActionRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(interactions);
        ArgumentNullException.ThrowIfNull(requirement);
        if (interactions.Any(i => i is null))
        {
            throw new ArgumentException("every interaction must be stated", nameof(interactions));
        }

        var costs = ImmutableArray.CreateBuilder<InteractionCost>();
        bool freeTaken = false;
        foreach (var interaction in interactions)
        {
            if (requirement.Requires(ActivityKind.ObjectInteraction, interaction.What))
            {
                costs.Add(new InteractionCost(
                    interaction,
                    InteractionCostKind.RequiresAction,
                    $"{requirement}, so this rule does not make it free",
                    MapEntries.GmRequiresAction.Locator));
                continue;
            }

            if (interaction.AlwaysRequiresAction)
            {
                costs.Add(new InteractionCost(
                    interaction,
                    InteractionCostKind.RequiresAction,
                    "the object always requires an action to use, as stated in its description",
                    MapEntries.FreeObjectInteraction.Locator));
                continue;
            }

            if (!freeTaken)
            {
                freeTaken = true;
                costs.Add(new InteractionCost(
                    interaction,
                    InteractionCostKind.Free,
                    $"the one free interaction of the turn, {interaction.During}",
                    MapEntries.FreeObjectInteraction.Locator));
                continue;
            }

            costs.Add(new InteractionCost(
                interaction,
                InteractionCostKind.RequiresUtilizeAction,
                "a second object is interacted with, which needs the Utilize action",
                MapEntries.FreeObjectInteraction.Locator));
        }

        return Resolution<TurnInteractions>.FromValue(
            new TurnInteractions(costs.ToImmutable(), requirement, MapEntries.FreeObjectInteraction.Locator));
    }

    /// <summary>
    /// What communicating costs: "You can communicate however you are able—through brief utterances
    /// and gestures—as you take your turn. Doing so uses neither your action nor your move. Extended
    /// communication… requires an action." Which of the two a communication is, is
    /// <c>brief-or-extended-communication</c>, a gap; the caller classifies it and the rule gives
    /// the cost.
    /// </summary>
    /// <remarks>
    /// Where the GM's requirement names this very communication, the answer turns on the entry's own
    /// unresolved question — whether "any of these activities" reaches communication at all — so the
    /// rule declines <see cref="UnresolvedReason.RequiresInterpretation"/> citing
    /// <c>communication-cost</c> and chooses neither reading. A requirement naming only object
    /// interactions leaves this rule untouched.
    /// </remarks>
    /// <param name="communication">The communication, classified by the caller.</param>
    /// <param name="requirement">The GM's requirement, the assertion <c>gm-requires-action</c>; never defaulted.</param>
    /// <returns>The cost, or the decline.</returns>
    public static Resolution<CommunicationCost> Communicates(CommunicationOnTurn communication, GmActionRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(communication);
        ArgumentNullException.ThrowIfNull(requirement);
        if (requirement.Requires(ActivityKind.Communication, communication.What))
        {
            return Resolution<CommunicationCost>.FromUnresolved(new UnresolvedResult(
                UnresolvedReason.RequiresInterpretation,
                $"resolve the map entry '{MapEntries.CommunicationCost.Id}' [{MapEntries.CommunicationCost.Locator.Citation}] "
                + $"for {communication} while {requirement}: whether the GM's requirement reaches communication is not stated",
                MapEntries.CommunicationCost.Locator));
        }

        var (cost, why) = communication.Kind == CommunicationKind.Brief
            ? (CommunicationCostKind.Free, "brief communication uses neither the action nor the move")
            : (CommunicationCostKind.RequiresAction, "extended communication requires an action");
        return Resolution<CommunicationCost>.FromValue(
            new CommunicationCost(communication, cost, why, requirement, MapEntries.CommunicationCost.Locator));
    }

    private static TakenTurn Taken(int speed, IReadOnlyList<TurnStep> steps, string statedBy)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(speed);
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentException.ThrowIfNullOrWhiteSpace(statedBy);
        if (steps.Any(s => s is null))
        {
            throw new ArgumentException("every step of the turn must be stated", nameof(steps));
        }

        int used = steps.Where(s => s.Kind == TurnStepKind.Move).Sum(s => s.Feet);
        int actions = steps.Count(s => s.Kind == TurnStepKind.Action);
        var exceeded = ImmutableArray.CreateBuilder<string>();
        if (used > speed)
        {
            exceeded.Add($"a move of {used} feet, and the turn allows a distance up to the Speed of {speed}");
        }

        if (actions > 1)
        {
            exceeded.Add($"{actions} actions, and the turn allows one");
        }

        return new TakenTurn(
            speed,
            [.. steps],
            used,
            actions,
            exceeded.ToImmutable(),
            statedBy,
            MapEntries.TurnMoveAndAction.Locator);
    }
}
