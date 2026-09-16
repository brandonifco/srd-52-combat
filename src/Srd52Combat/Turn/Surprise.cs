using RulesKernel.Provenance;
using Srd52Combat.Initiative;

namespace Srd52Combat.Turn;

/// <summary>
/// Whether a combatant is surprised by combat starting, as the caller states it. When a combatant
/// is surprised is <c>surprised</c> ("Combat / Initiative / p. 13"), a gap the map leaves
/// unresolved — the corpus gives one sufficient example and no measure — so the engine decides it
/// for nobody and takes the fact, attributed. <c>surprise-disadvantage</c> is the computable half:
/// given the fact, what follows.
/// </summary>
/// <param name="CombatantId">The combatant.</param>
/// <param name="IsSurprised">True when that combatant is surprised by combat starting.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record SurprisedStatement(string CombatantId, bool IsSurprised, string StatedBy)
{
    /// <summary>The combatant, checked to be non-empty.</summary>
    public string CombatantId { get; } = Checks.Text(CombatantId, nameof(CombatantId));

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <inheritdoc/>
    public override string ToString() =>
        IsSurprised
            ? $"{CombatantId} is surprised by combat starting, as stated by {StatedBy}"
            : $"{CombatantId} is not surprised by combat starting, as stated by {StatedBy}";
}

/// <summary>
/// What surprise does to a combatant (<c>surprise-disadvantage</c>, "Combat / Initiative / p. 13"):
/// "If a combatant is surprised by combat starting, that combatant has Disadvantage on their
/// Initiative roll." Nothing else follows: this corpus's only consequence of surprise is that
/// Disadvantage (<c>surprise-round</c> is absent from it).
/// </summary>
/// <param name="Statement">The fact this rests on, as the caller stated it.</param>
/// <param name="DisadvantageOnInitiativeRoll">True when this rule gives the combatant's Initiative roll Disadvantage.</param>
/// <param name="Authority">The rule: <c>surprise-disadvantage</c>, "Combat / Initiative / p. 13".</param>
public sealed record SurpriseEffect(SurprisedStatement Statement, bool DisadvantageOnInitiativeRoll, SourceLocator Authority)
{
    /// <summary>The combatant.</summary>
    public string CombatantId => Statement.CombatantId;

    /// <summary>
    /// What this rule contributes to the combatant's roll mode: <see cref="D20Mode.Disadvantage"/>
    /// when it is surprised, and null when it is not. Null is not
    /// <see cref="D20Mode.Straight"/>: a roll's mode comes from every source (the Incapacitated and
    /// Invisible conditions among them), and this rule speaks only of surprise.
    /// </summary>
    public D20Mode? InitiativeRoll => DisadvantageOnInitiativeRoll ? D20Mode.Disadvantage : null;

    /// <inheritdoc/>
    public override string ToString() =>
        DisadvantageOnInitiativeRoll
            ? $"{CombatantId} has Disadvantage on their Initiative roll [{Authority.Citation}]"
            : $"{CombatantId} gets nothing from this rule [{Authority.Citation}]";
}
