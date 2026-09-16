using System.Collections.Immutable;
using RulesKernel.Randomness;
using RulesKernel.Resolution;
using Srd52Combat.Initiative;

namespace Srd52Combat.Rules;

/// <summary>
/// Initiative, "Combat / Initiative / p. 13": the roll (<c>initiative-roll</c>), the order
/// (<c>initiative-order</c>), who breaks a tie (<c>initiative-ties</c>), and the ties nobody is
/// assigned (<c>initiative-ties-uncovered</c>). Every entry cites the same page, so every decline
/// names its entry in <see cref="UnresolvedResult.Attempted"/>.
/// </summary>
public static class InitiativeRules
{
    private const int D20Faces = 20;

    /// <summary>
    /// Every participant rolls Initiative: one d20 each, drawn from <paramref name="source"/> in the
    /// order the participants are given, plus the Dexterity check modifier the caller stated.
    /// </summary>
    /// <remarks>
    /// Nothing is drawn unless every statement allows a plain roll per participant, and each case
    /// that would draw differently declines first:
    /// <list type="bullet">
    /// <item>the GM uses Initiative scores: <see cref="UnresolvedReason.OutsideCurrentScope"/>, citing
    /// <c>initiative-score-option</c> (p. 184), the gate outside the slice that suspends this entry;</item>
    /// <item>a group of identical creatures, for which the GM makes a single roll:
    /// <see cref="UnresolvedReason.RequiresInterpretation"/>, citing <c>group-initiative</c>, which is built
    /// and declines — map 2.0.0's <c>draws</c> counts one d20 per participant "not in a group of identical
    /// creatures, whose roll is group-initiative's", and what makes such a group the corpus never says
    /// (decision 0005);</item>
    /// <item>a roll with Advantage, Disadvantage, or both: <see cref="UnresolvedReason.OutsideCurrentScope"/>,
    /// citing <c>advantage-disadvantage</c> (p. 7). Map 2.0.0's <c>draws</c> counts one d20 for a roll with
    /// both, because they cancel; the cancelling is that <c>scope: out</c> entry's rule (decision 0002).</item>
    /// </list>
    /// </remarks>
    /// <param name="participants">Every participant, as the caller states them.</param>
    /// <param name="statedBy">Who stated the participants.</param>
    /// <param name="scoreOption">Whether the GM uses Initiative scores. Never defaulted.</param>
    /// <param name="identicalCreatures">Which participants form groups of identical creatures. Never defaulted.</param>
    /// <param name="source">The seeded generator.</param>
    /// <returns>The rolls, or the decline.</returns>
    public static Resolution<InitiativeRolls> Roll(
        IReadOnlyList<Combatant> participants,
        string statedBy,
        InitiativeScoreOptionStatement scoreOption,
        IdenticalCreaturesStatement identicalCreatures,
        IRandomSource source)
    {
        ArgumentNullException.ThrowIfNull(participants);
        ArgumentException.ThrowIfNullOrWhiteSpace(statedBy);
        ArgumentNullException.ThrowIfNull(scoreOption);
        ArgumentNullException.ThrowIfNull(identicalCreatures);
        ArgumentNullException.ThrowIfNull(source);
        if (participants.Count == 0 || participants.Any(p => p is null))
        {
            throw new ArgumentException("combat has at least one participant, and none is null", nameof(participants));
        }

        var ids = participants.Select(p => p.Id).ToImmutableArray();
        if (ids.Distinct(StringComparer.Ordinal).Count() != ids.Length)
        {
            throw new ArgumentException("every participant has its own id", nameof(participants));
        }

        var unknown = identicalCreatures.Groups.SelectMany(g => g).Where(id => !ids.Contains(id, StringComparer.Ordinal)).ToArray();
        if (unknown.Length > 0)
        {
            throw new ArgumentException($"the identical-creatures statement names {string.Join(", ", unknown)}, who are not participants", nameof(identicalCreatures));
        }

        string attempted = $"resolve the map entry '{MapEntries.InitiativeRoll.Id}' [{MapEntries.InitiativeRoll.Locator.Citation}]";
        if (scoreOption.InUse)
        {
            return Decline(UnresolvedReason.OutsideCurrentScope, $"{attempted} while {scoreOption}", MapEntries.InitiativeScoreOption);
        }

        if (!identicalCreatures.Groups.IsEmpty)
        {
            return Decline(
                UnresolvedReason.RequiresInterpretation,
                $"{attempted} while {identicalCreatures}: the roll of a participant in a group is '{MapEntries.GroupInitiative.Id}'"
                + " [map draws], whose question the corpus leaves open, so how many d20s this combat throws is not fixed",
                MapEntries.GroupInitiative);
        }

        var modified = participants.Where(p => p.Roll != D20Mode.Straight).ToArray();
        if (modified.Length > 0)
        {
            return Decline(
                UnresolvedReason.OutsideCurrentScope,
                $"{attempted} while {string.Join(", ", modified.Select(p => $"{p.Id}'s roll has {p.Roll}"))}, as stated by {statedBy}",
                MapEntries.AdvantageDisadvantage);
        }

        var rolls = participants
            .Select(p => new RolledInitiative(p.Id, p.Kind, UniformInt.InRange(source, 1, D20Faces), p.DexterityCheckModifier))
            .ToImmutableArray();
        return Resolution<InitiativeRolls>.FromValue(
            new InitiativeRolls(rolls, statedBy, scoreOption, identicalCreatures, MapEntries.InitiativeRoll.Locator));
    }

    /// <summary>
    /// Who the tie rule says decides each tie among <paramref name="counts"/>: the GM among tied
    /// monsters, the players among tied characters, the GM between monsters and player characters.
    /// Each decider is a party the map's <c>assertedBy</c> for <c>initiative-ties</c> names (0025).
    /// A tie between a monster and a character that is not a player character is assigned to nobody,
    /// and declines <see cref="UnresolvedReason.RequiresInterpretation"/> citing
    /// <c>initiative-ties-uncovered</c>.
    /// </summary>
    /// <param name="counts">Every combatant's Initiative.</param>
    /// <returns>Each tie, highest Initiative first, and its decider; or the decline.</returns>
    public static Resolution<ImmutableArray<TieAssignment>> Assign(IReadOnlyList<InitiativeCount> counts)
    {
        var ties = Ties(counts);
        var assignments = ImmutableArray.CreateBuilder<TieAssignment>();
        foreach (var tie in ties)
        {
            bool monster = tie.Any(c => c.Kind == CombatantKind.Monster);
            bool nonPlayerCharacter = tie.Any(c => c.Kind == CombatantKind.NonPlayerCharacter);
            if (monster && nonPlayerCharacter)
            {
                return Resolution<ImmutableArray<TieAssignment>>.FromUnresolved(new UnresolvedResult(
                    UnresolvedReason.RequiresInterpretation,
                    $"resolve the map entry '{MapEntries.InitiativeTiesUncovered.Id}' [{MapEntries.InitiativeTiesUncovered.Locator.Citation}] "
                    + $"for the tie at {tie[0].Initiative} among {Describe(tie)}: the tie rule does not say who orders a monster and a character that is not a player character",
                    MapEntries.InitiativeTiesUncovered.Locator));
            }

            // Every tied combatant a monster: the GM. No monster: all characters, the players.
            // Monsters and player characters only: the GM.
            // Each party is looked up in the map's assertedBy for initiative-ties (0025).
            var decider = TieDeciders.Named(monster ? "GM" : "players");
            assignments.Add(new TieAssignment(tie[0].Initiative, [.. tie.Select(c => c.CombatantId)], decider, MapEntries.InitiativeTies.Locator));
        }

        return Resolution<ImmutableArray<TieAssignment>>.FromValue(assignments.ToImmutable());
    }

    /// <summary>
    /// The tie breaks for <paramref name="counts"/>, checked against the tie rule: one per tie,
    /// ordering exactly the tied combatants, by the decider the rule names, a party the map's
    /// <c>assertedBy</c> names. The statement is demanded only when there is a tie, and never inferred.
    /// </summary>
    /// <param name="counts">Every combatant's Initiative.</param>
    /// <param name="asserted">Reads the caller's <c>initiative-ties</c> assertion; throws when none was made.</param>
    /// <returns>The tie breaks, in tie order; or a decline citing <c>initiative-ties-uncovered</c>.</returns>
    /// <exception cref="ArgumentException">A tie break is missing, extra, of other combatants, or by the wrong decider.</exception>
    public static Resolution<ImmutableArray<TieBreak>> BreakTies(IReadOnlyList<InitiativeCount> counts, Func<TieBreaks> asserted)
    {
        ArgumentNullException.ThrowIfNull(asserted);
        return Assign(counts).Match(
            assignments => assignments.IsEmpty
                ? Resolution<ImmutableArray<TieBreak>>.FromValue([])
                : Match(assignments, asserted()),
            Resolution<ImmutableArray<TieBreak>>.FromUnresolved);
    }

    /// <summary>
    /// The Initiative order: from highest to lowest Initiative, each tie in the order its tie break
    /// states, the same in every round.
    /// </summary>
    /// <param name="counts">Every combatant's Initiative.</param>
    /// <param name="asserted">Reads the caller's <c>initiative-ties</c> assertion; read only when there is a tie.</param>
    /// <returns>The order, or a decline citing <c>initiative-ties-uncovered</c>.</returns>
    public static Resolution<TurnOrder> Order(IReadOnlyList<InitiativeCount> counts, Func<TieBreaks> asserted) =>
        BreakTies(counts, asserted).Match(
            breaks =>
            {
                var position = breaks
                    .SelectMany(b => b.Order.Select((id, index) => (id, index)))
                    .ToDictionary(p => p.id, p => p.index, StringComparer.Ordinal);
                var order = counts
                    .OrderByDescending(c => c.Initiative)
                    .ThenBy(c => position.GetValueOrDefault(c.CombatantId))
                    .ToImmutableArray();
                return Resolution<TurnOrder>.FromValue(new TurnOrder(order, breaks, MapEntries.InitiativeOrder.Locator));
            },
            Resolution<TurnOrder>.FromUnresolved);

    private static Resolution<ImmutableArray<TieBreak>> Match(ImmutableArray<TieAssignment> assignments, TieBreaks stated)
    {
        ArgumentNullException.ThrowIfNull(stated);
        var remaining = stated.Items.ToList();
        var breaks = ImmutableArray.CreateBuilder<TieBreak>();
        foreach (var tie in assignments)
        {
            var matching = remaining.Where(b => b.Order.ToHashSet(StringComparer.Ordinal).SetEquals(tie.Tied)).ToArray();
            if (matching.Length != 1)
            {
                throw new ArgumentException(
                    $"the tie at {tie.Initiative} among {string.Join(", ", tie.Tied)} needs exactly one tie break ordering exactly those combatants, and {matching.Length} were stated",
                    nameof(stated));
            }

            var tieBreak = matching[0];
            remaining.Remove(tieBreak);
            // The attribution is checked against the map's assertedBy for initiative-ties (0025): the
            // stated party must be one the map names, and the one it names for this tie.
            string statedParty = TieDeciders.AssertedBy(tieBreak.DecidedBy);
            if (!MapEntries.InitiativeTies.AssertedBy.Contains(statedParty, StringComparer.Ordinal))
            {
                throw new ArgumentException(
                    $"'{MapEntries.InitiativeTies.Id}' is asserted by {string.Join(", ", MapEntries.InitiativeTies.AssertedBy)} [map assertedBy], and the statement says {statedParty} decided the tie at {tie.Initiative}",
                    nameof(stated));
            }

            if (statedParty != TieDeciders.AssertedBy(tie.Decider))
            {
                throw new ArgumentException(
                    $"the tie at {tie.Initiative} among {string.Join(", ", tie.Tied)} is the {tie.Decider}'s to decide [{tie.Authority.Citation}], and the statement says {tieBreak.DecidedBy} decided it",
                    nameof(stated));
            }

            if (tieBreak.PlayersDidNotAgree)
            {
                return Resolution<ImmutableArray<TieBreak>>.FromUnresolved(new UnresolvedResult(
                    UnresolvedReason.RequiresInterpretation,
                    $"resolve the map entry '{MapEntries.InitiativeTiesUncovered.Id}' [{MapEntries.InitiativeTiesUncovered.Locator.Citation}] "
                    + $"for the tie at {tie.Initiative} while {tieBreak}: the tie rule does not say how tied characters are ordered when the players do not agree",
                    MapEntries.InitiativeTiesUncovered.Locator));
            }

            breaks.Add(tieBreak);
        }

        if (remaining.Count > 0)
        {
            throw new ArgumentException($"no tie exists for the tie break(s) {string.Join("; ", remaining)}", nameof(stated));
        }

        return Resolution<ImmutableArray<TieBreak>>.FromValue(breaks.ToImmutable());
    }

    private static ImmutableArray<ImmutableArray<InitiativeCount>> Ties(IReadOnlyList<InitiativeCount> counts)
    {
        ArgumentNullException.ThrowIfNull(counts);
        if (counts.Count == 0 || counts.Any(c => c is null))
        {
            throw new ArgumentException("an Initiative order has at least one combatant, and none is null", nameof(counts));
        }

        if (counts.Select(c => c.CombatantId).Distinct(StringComparer.Ordinal).Count() != counts.Count)
        {
            throw new ArgumentException("every combatant has its own id", nameof(counts));
        }

        return [.. counts
            .GroupBy(c => c.Initiative)
            .Where(g => g.Count() > 1)
            .OrderByDescending(g => g.Key)
            .Select(g => g.ToImmutableArray())];
    }

    private static string Describe(IEnumerable<InitiativeCount> tie) =>
        string.Join(", ", tie.Select(c => $"{c.CombatantId} ({c.Kind})"));

    private static Resolution<InitiativeRolls> Decline(UnresolvedReason reason, string attempted, MapEntry cited) =>
        Resolution<InitiativeRolls>.FromUnresolved(new UnresolvedResult(reason, attempted, cited.Locator));
}
