using System.Collections.Immutable;
using RulesKernel.Provenance;
using Srd52Combat.Initiative;

namespace Srd52Combat.Turn;

/// <summary>Where one creature is, as step 1 of combat establishes it.</summary>
/// <param name="CombatantId">The character or monster.</param>
/// <param name="Where">Where it is: "how far away and in what direction", in the words of whoever states it.</param>
public sealed record Position(string CombatantId, string Where)
{
    /// <summary>The combatant, checked to be non-empty.</summary>
    public string CombatantId { get; } = Checks.Text(CombatantId, nameof(CombatantId));

    /// <summary>The position, checked to be non-empty.</summary>
    public string Where { get; } = Checks.Text(Where, nameof(Where));

    /// <inheritdoc/>
    public override string ToString() => $"{CombatantId}: {Where}";
}

/// <summary>
/// Step 1 of combat: "The Game Master determines where all the characters and monsters are
/// located." Positions are facts the later rules test (range, reach, cover, adjacency), so they are
/// parameters and the engine decides none of them; it records them and who supplied them.
/// </summary>
/// <param name="Positions">Where each character and monster is.</param>
/// <param name="StatedBy">Who supplied them, the GM in play.</param>
public sealed record PositionsStatement(ImmutableArray<Position> Positions, string StatedBy)
{
    /// <summary>The positions, checked to be present and each stated.</summary>
    public ImmutableArray<Position> Positions { get; } =
        Positions.IsDefault || Positions.Any(p => p is null)
            ? throw new ArgumentException("the positions must be stated, even when there are none", nameof(Positions))
            : Positions;

    /// <summary>Who stated them, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>These positions.</summary>
    /// <param name="statedBy">Who supplied them.</param>
    /// <param name="positions">Where each character and monster is.</param>
    /// <returns>The statement.</returns>
    public static PositionsStatement Of(string statedBy, params Position[] positions) => new([.. positions], statedBy);

    /// <inheritdoc/>
    public override string ToString() => $"positions stated by {StatedBy}: {string.Join("; ", Positions)}";
}

/// <summary>One of the three steps combat unfolds in.</summary>
/// <param name="Number">1, 2 or 3, in the corpus's order.</param>
/// <param name="Name">The step's name, as the corpus names it.</param>
/// <param name="What">What the step is, in this combat.</param>
public sealed record CombatStep(int Number, string Name, string What)
{
    /// <inheritdoc/>
    public override string ToString() => $"{Number}: {Name}. {What}";
}

/// <summary>
/// Combat step by step (<c>combat-steps</c>, "Combat / Combat Step by Step / p. 13"): the positions
/// the GM established, the Initiative that was rolled and ordered, and the turns each round takes
/// in that order. Whether another round follows is not this rule's: "Repeat this step until the
/// fighting stops" names no rule of its own, and when fighting stops is <c>next-round</c> and
/// <c>combat-end</c>, which conflict.
/// </summary>
/// <param name="Positions">Step 1, as stated.</param>
/// <param name="Order">Step 2's result: the Initiative order (<c>initiative-order</c>).</param>
/// <param name="Steps">The three steps, in the corpus's order.</param>
/// <param name="Authority">The rule: <c>combat-steps</c>, "Combat / Combat Step by Step / p. 13".</param>
public sealed record CombatStepByStep(
    PositionsStatement Positions,
    TurnOrder Order,
    ImmutableArray<CombatStep> Steps,
    SourceLocator Authority)
{
    /// <summary>Step 3 for one round: "Each participant in the battle takes a turn in Initiative order."</summary>
    /// <param name="round">The round, from 1. That the round happens at all is not this rule's to say.</param>
    /// <returns>The combatant ids, first to act first.</returns>
    public ImmutableArray<string> TurnsInRound(int round) => Order.TurnsInRound(round);

    /// <inheritdoc/>
    public override string ToString() => string.Join(" ", Steps);
}

/// <summary>
/// Whether a side has been defeated, as the caller states it. What defeat is, and who is on which
/// side, is <c>side-defeated</c> ("Combat / Ending Combat / p. 14"), a gap the map leaves
/// unresolved; the engine decides neither and takes the fact, attributed.
/// </summary>
/// <param name="ASideIsDefeated">True when a side has been defeated.</param>
/// <param name="Side">Which side, in the caller's words; null when none is defeated.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record SideDefeatedStatement(bool ASideIsDefeated, string? Side, string StatedBy)
{
    /// <summary>The side, checked to be named exactly when there is one.</summary>
    public string? Side { get; } = ASideIsDefeated
        ? Checks.Text(Side!, nameof(Side))
        : Side is null ? null : throw new ArgumentException("no side is defeated, so none is named", nameof(Side));

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>Neither side has been defeated.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static SideDefeatedStatement Neither(string statedBy) => new(ASideIsDefeated: false, null, statedBy);

    /// <summary>This side has been defeated.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <param name="side">The side.</param>
    /// <returns>The statement.</returns>
    public static SideDefeatedStatement Defeated(string statedBy, string side) => new(ASideIsDefeated: true, side, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        ASideIsDefeated
            ? $"{Side} is defeated, as stated by {StatedBy}"
            : $"neither side is defeated, as stated by {StatedBy}";
}

/// <summary>
/// Whether both sides have agreed to end combat, as the caller states it. The agreement is
/// <c>sides-agree-to-end</c> ("Combat / Ending Combat / p. 14"), which this engine has not built;
/// <c>next-round</c> reads the fact only to see whether its question arises, and never infers one.
/// </summary>
/// <param name="AgreedToEnd">True when both sides have agreed to end combat.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record SidesAgreementStatement(bool AgreedToEnd, string StatedBy)
{
    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The sides have not agreed to end combat.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static SidesAgreementStatement NotAgreed(string statedBy) => new(AgreedToEnd: false, statedBy);

    /// <summary>Both sides have agreed to end combat.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static SidesAgreementStatement Agreed(string statedBy) => new(AgreedToEnd: true, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        AgreedToEnd
            ? $"both sides agree to end combat, as stated by {StatedBy}"
            : $"the sides have not agreed to end combat, as stated by {StatedBy}";
}

/// <summary>
/// What follows the turns of one round (<c>next-round</c>, "Combat / The Order of Combat / p. 13"):
/// "Once everyone has taken a turn, the fight continues to the next round if neither side is
/// defeated."
/// </summary>
/// <param name="Round">The round the turns were taken in.</param>
/// <param name="RoundOver">Whether everyone has taken a turn.</param>
/// <param name="Continues">Whether the fight continues to another round.</param>
/// <param name="Next">The round that follows, or null when none does.</param>
/// <param name="Why">The rule's own reason, in the engine's words.</param>
/// <param name="Waiting">The combatants who have not yet taken a turn, in Initiative order.</param>
/// <param name="Authority">The rule: <c>next-round</c>, "Combat / The Order of Combat / p. 13".</param>
/// <param name="Rulings">
/// The owner's rulings this answer relies on: <c>next-round/agreement-ends-it</c> where both sides
/// agreed and neither is defeated, which is the case p. 13 and p. 14 answer differently, and none in
/// the three cases the corpus settles on its own.
/// </param>
public sealed record NextRoundOutcome(
    int Round,
    bool RoundOver,
    bool Continues,
    int? Next,
    string Why,
    ImmutableArray<string> Waiting,
    SourceLocator Authority,
    ImmutableArray<OwnerRuling> Rulings)
{
    /// <summary>The rulings, checked to be present, even when there are none.</summary>
    public ImmutableArray<OwnerRuling> Rulings { get; } =
        Rulings.IsDefault ? throw new ArgumentNullException(nameof(Rulings)) : Rulings;

    /// <inheritdoc/>
    public override string ToString() => $"round {Round}: {Why} [{Authority.Citation}]";
}
