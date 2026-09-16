using System.Collections.Immutable;
using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Turn;

namespace Srd52Combat.Rules;

/// <summary>
/// The shape of a combat: the three steps it unfolds in (<c>combat-steps</c>, "Combat / Combat Step
/// by Step / p. 13") and what follows the turns of a round (<c>next-round</c>, "Combat / The Order
/// of Combat / p. 13").
/// </summary>
public static class RoundRules
{
    /// <summary>
    /// Combat step by step: "1: Establish Positions… 2: Roll Initiative… 3: Take Turns. Each
    /// participant in the battle takes a turn in Initiative order."
    /// </summary>
    /// <remarks>
    /// The steps are in the corpus's order, and step 3 rests on step 2: the turns are the Initiative
    /// order's, so the sequence cannot be built without the order <c>initiative-order</c> resolved.
    /// Asking for the steps without one is refused (<see cref="ArgumentException"/>) and no turn is
    /// taken. Step 1 is the caller's statement, recorded and attributed. Whether the fighting stops
    /// is not answered here: "Repeat this step until the fighting stops" names no rule of its own.
    /// </remarks>
    /// <param name="positions">Step 1: where the characters and monsters are, as stated.</param>
    /// <param name="order">Step 2's result: the Initiative order.</param>
    /// <returns>The three steps, and the turns of any round in Initiative order.</returns>
    public static Resolution<CombatStepByStep> Steps(PositionsStatement positions, TurnOrder order)
    {
        ArgumentNullException.ThrowIfNull(positions);
        ArgumentNullException.ThrowIfNull(order);
        var turns = order.TurnsInRound(1);
        ImmutableArray<CombatStep> steps =
        [
            new(1, "Establish Positions", $"where all the characters and monsters are located, as stated by {positions.StatedBy}: {string.Join("; ", positions.Positions)}"),
            new(2, "Roll Initiative", $"Initiative is rolled and ordered [{order.Authority.Citation}]: {string.Join(", ", turns)}"),
            new(3, "Take Turns", $"each participant takes a turn in Initiative order; the round ends when all {turns.Length} have had one"),
        ];
        return Resolution<CombatStepByStep>.FromValue(
            new CombatStepByStep(positions, order, steps, MapEntries.CombatSteps.Locator));
    }

    /// <summary>
    /// What follows the turns of a round: "Once everyone has taken a turn, the fight continues to
    /// the next round if neither side is defeated."
    /// </summary>
    /// <remarks>
    /// Before everyone has taken a turn the round is not over and no next round follows yet. Once it
    /// is over: with neither side defeated the fight continues to the round after this one; with a
    /// side defeated it does not. Where neither side is defeated and both sides have agreed to end
    /// combat, p. 14 says combat can end and this sentence says another round begins; that conflict
    /// is the entry's unresolved question, so the rule declines
    /// <see cref="UnresolvedReason.RequiresInterpretation"/> citing <c>next-round</c> and chooses
    /// neither reading.
    /// </remarks>
    /// <param name="round">The round whose turns were taken, from 1.</param>
    /// <param name="order">Every combatant, in Initiative order.</param>
    /// <param name="turnsTaken">The combatants that have taken their turn in this round.</param>
    /// <param name="defeat">Whether a side is defeated, as stated; never defaulted.</param>
    /// <param name="agreement">Whether both sides have agreed to end combat, as stated; never defaulted.</param>
    /// <returns>Whether another round follows, or the decline.</returns>
    /// <exception cref="ArgumentException">The order is empty or repeats a combatant, or a turn is taken by someone not in it or twice.</exception>
    public static Resolution<NextRoundOutcome> NextRound(
        int round,
        IReadOnlyList<string> order,
        IReadOnlyList<string> turnsTaken,
        SideDefeatedStatement defeat,
        SidesAgreementStatement agreement)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(round, 1);
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(turnsTaken);
        ArgumentNullException.ThrowIfNull(defeat);
        ArgumentNullException.ThrowIfNull(agreement);
        if (order.Count == 0 || order.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("a combat has at least one combatant, and each is named", nameof(order));
        }

        if (order.Distinct(StringComparer.Ordinal).Count() != order.Count)
        {
            throw new ArgumentException("every combatant has its own id", nameof(order));
        }

        var taken = new HashSet<string>(StringComparer.Ordinal);
        foreach (string id in turnsTaken)
        {
            if (!order.Contains(id, StringComparer.Ordinal))
            {
                throw new ArgumentException($"{id} took a turn and is not in the Initiative order", nameof(turnsTaken));
            }

            if (!taken.Add(id))
            {
                throw new ArgumentException($"{id} took two turns in round {round}", nameof(turnsTaken));
            }
        }

        var waiting = order.Where(id => !taken.Contains(id)).ToImmutableArray();
        if (waiting.Length > 0)
        {
            return Resolution<NextRoundOutcome>.FromValue(new NextRoundOutcome(
                round,
                RoundOver: false,
                Continues: false,
                Next: null,
                $"not everyone has taken a turn in round {round}: {string.Join(", ", waiting)} still to act",
                waiting,
                MapEntries.NextRound.Locator));
        }

        if (!defeat.ASideIsDefeated && agreement.AgreedToEnd)
        {
            return Resolution<NextRoundOutcome>.FromUnresolved(new UnresolvedResult(
                UnresolvedReason.RequiresInterpretation,
                $"resolve the map entry '{MapEntries.NextRound.Id}' [{MapEntries.NextRound.Locator.Citation}] "
                + $"after round {round}, while {defeat} and {agreement}: this rule continues the fight and "
                + $"'{MapEntries.CombatEnd.Id}' [{MapEntries.CombatEnd.Locator.Citation}] ends the combat, and the corpus does not say which governs",
                MapEntries.NextRound.Locator));
        }

        return defeat.ASideIsDefeated
            ? Resolution<NextRoundOutcome>.FromValue(new NextRoundOutcome(
                round,
                RoundOver: true,
                Continues: false,
                Next: null,
                $"everyone has taken a turn and {defeat}, so the fight does not continue to another round",
                [],
                MapEntries.NextRound.Locator))
            : Resolution<NextRoundOutcome>.FromValue(new NextRoundOutcome(
                round,
                RoundOver: true,
                Continues: true,
                Next: round + 1,
                $"everyone has taken a turn and {defeat}, so the fight continues to round {round + 1}",
                [],
                MapEntries.NextRound.Locator));
    }
}
