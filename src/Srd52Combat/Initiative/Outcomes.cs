using System.Collections.Immutable;
using RulesKernel.Provenance;

namespace Srd52Combat.Initiative;

/// <summary>One combatant's Initiative roll: the d20 drawn and the total.</summary>
/// <param name="CombatantId">The combatant.</param>
/// <param name="Kind">Monster, player character, or another character.</param>
/// <param name="D20">The face drawn, 1 to 20.</param>
/// <param name="Modifier">The Dexterity check modifier the caller stated.</param>
public sealed record RolledInitiative(string CombatantId, CombatantKind Kind, int D20, int Modifier)
{
    /// <summary>The check total, the combatant's Initiative.</summary>
    public int Total => D20 + Modifier;

    /// <summary>The Initiative count this roll gives.</summary>
    public InitiativeCount Count => new(CombatantId, Kind, Total);

    /// <inheritdoc/>
    public override string ToString() => $"{CombatantId} ({Kind}) d20={D20} {Modifier:+0;-0;+0} = {Total}";
}

/// <summary>Every participant's Initiative roll, in the order the participants were given, and what the caller stated.</summary>
/// <param name="Rolls">One roll per participant.</param>
/// <param name="StatedBy">Who stated the participants: their kinds, modifiers and roll modes.</param>
/// <param name="ScoreOption">The Initiative-score statement the roll was made under.</param>
/// <param name="IdenticalCreatures">The identical-creatures statement the roll was made under.</param>
/// <param name="Authority">The rule: <c>initiative-roll</c>, "Combat / Initiative / p. 13".</param>
public sealed record InitiativeRolls(
    ImmutableArray<RolledInitiative> Rolls,
    string StatedBy,
    InitiativeScoreOptionStatement ScoreOption,
    IdenticalCreaturesStatement IdenticalCreatures,
    SourceLocator Authority)
{
    /// <summary>The Initiative counts, in the order the participants were given.</summary>
    public ImmutableArray<InitiativeCount> Counts => [.. Rolls.Select(r => r.Count)];
}

/// <summary>Who decides the order among a set of tied combatants.</summary>
public enum TieDecider
{
    /// <summary>The GM.</summary>
    Gm = 1,

    /// <summary>The players.</summary>
    Players = 2,
}

/// <summary>
/// The caller's statement breaking one Initiative tie: the order the decider chose, attributed, or
/// (for a tie among characters) that the players did not agree on one. The value of the assertion
/// <c>initiative-ties</c> is a <see cref="TieBreaks"/> holding one of these per tie.
/// </summary>
/// <param name="DecidedBy">Who decided: the GM or the players.</param>
/// <param name="Order">The tied combatants, first to act first; exactly the tied set.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
/// <param name="PlayersDidNotAgree">True when the players state they did not agree on an order; <paramref name="Order"/> then lists the tied set in any order.</param>
public sealed record TieBreak(TieDecider DecidedBy, ImmutableArray<string> Order, string StatedBy, bool PlayersDidNotAgree = false)
{
    /// <summary>The decider, checked to be stated, and the players when they did not agree.</summary>
    public TieDecider DecidedBy { get; } =
        PlayersDidNotAgree && DecidedBy != TieDecider.Players
            ? throw new ArgumentException("only the players can fail to agree on a tie's order", nameof(DecidedBy))
            : Checks.Defined(DecidedBy, nameof(DecidedBy));

    /// <summary>The order, checked to name at least two distinct combatants.</summary>
    public ImmutableArray<string> Order { get; } =
        Order.IsDefault || Order.Length < 2 || Order.Distinct(StringComparer.Ordinal).Count() != Order.Length
            ? throw new ArgumentException("a tie break orders at least two distinct combatants", nameof(Order))
            : Order;

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The order <paramref name="decider"/> chose for tied combatants.</summary>
    /// <param name="decider">Who decided.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <param name="order">The tied combatants, first to act first.</param>
    /// <returns>The statement.</returns>
    public static TieBreak Ordered(TieDecider decider, string statedBy, params string[] order) =>
        new(decider, [.. order], statedBy);

    /// <summary>The players did not agree on an order for these tied characters.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <param name="tied">The tied characters.</param>
    /// <returns>The statement.</returns>
    public static TieBreak PlayersDisagree(string statedBy, params string[] tied) =>
        new(TieDecider.Players, [.. tied], statedBy, PlayersDidNotAgree: true);

    /// <inheritdoc/>
    public override string ToString() =>
        PlayersDidNotAgree
            ? $"the players did not agree on an order for {string.Join(", ", Order)}, as stated by {StatedBy}"
            : $"{DecidedBy} ordered {string.Join(", ", Order)}, as stated by {StatedBy}";
}

/// <summary>The value of the assertion <c>initiative-ties</c>: one <see cref="TieBreak"/> per Initiative tie.</summary>
/// <param name="Items">The tie breaks.</param>
public sealed record TieBreaks(ImmutableArray<TieBreak> Items)
{
    /// <summary>The tie breaks, checked to be present.</summary>
    public ImmutableArray<TieBreak> Items { get; } = Items.IsDefault ? throw new ArgumentNullException(nameof(Items)) : Items;

    /// <summary>No tie breaks.</summary>
    public static TieBreaks None { get; } = new([]);

    /// <summary>These tie breaks.</summary>
    /// <param name="items">The tie breaks.</param>
    /// <returns>The value.</returns>
    public static TieBreaks Of(params TieBreak[] items) => new([.. items]);

    /// <inheritdoc/>
    public override string ToString() => Items.IsEmpty ? "no tie breaks" : string.Join("; ", Items);
}

/// <summary>A tie and who the tie rule says decides it.</summary>
/// <param name="Initiative">The tied Initiative.</param>
/// <param name="Tied">The tied combatants, in the order they were given.</param>
/// <param name="Decider">The GM or the players.</param>
/// <param name="Authority">The rule: "Combat / Initiative / p. 13".</param>
public sealed record TieAssignment(int Initiative, ImmutableArray<string> Tied, TieDecider Decider, SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() => $"{Initiative}: {string.Join(", ", Tied)} decided by {Decider}";
}

/// <summary>The Initiative order: who acts when, the same in every round.</summary>
/// <param name="Order">The combatants' counts, first to act first.</param>
/// <param name="TieBreaks">The tie breaks the caller stated and the order follows; empty when there was no tie.</param>
/// <param name="Authority">The rule: <c>initiative-order</c>, "Combat / Initiative / p. 13".</param>
public sealed record TurnOrder(ImmutableArray<InitiativeCount> Order, ImmutableArray<TieBreak> TieBreaks, SourceLocator Authority)
{
    /// <summary>
    /// The combatants in the order they act in round <paramref name="round"/>. "The Initiative order
    /// remains the same from round to round", so it is <see cref="Order"/> for every round.
    /// </summary>
    /// <param name="round">The round, from 1.</param>
    /// <returns>The combatant ids, first to act first.</returns>
    public ImmutableArray<string> TurnsInRound(int round)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(round, 1);
        return [.. Order.Select(c => c.CombatantId)];
    }
}
