using RulesKernel.Provenance;
using Srd52Combat.Initiative;

namespace Srd52Combat.Turn;

/// <summary>
/// Which kind of communication the caller states this is. Where brief communication ends and
/// extended communication begins is <c>brief-or-extended-communication</c> ("Combat / Your Turn /
/// p. 13"), a gap the map leaves unresolved and this engine does not decide: the classification is
/// stated, and <c>communication-cost</c> gives the cost that follows from it. There is no default.
/// </summary>
public enum CommunicationKind
{
    /// <summary>Brief: "brief utterances and gestures".</summary>
    Brief = 1,

    /// <summary>Extended: "a detailed explanation of something or an attempt to persuade a foe".</summary>
    Extended = 2,
}

/// <summary>One thing a creature communicates on its turn, classified by the caller.</summary>
/// <param name="Kind">Brief or extended, as the caller classifies it.</param>
/// <param name="What">What was communicated, in the caller's words.</param>
/// <param name="StatedBy">Who is answerable for the classification.</param>
public sealed record CommunicationOnTurn(CommunicationKind Kind, string What, string StatedBy)
{
    /// <summary>The kind, checked to be stated.</summary>
    public CommunicationKind Kind { get; } = Checks.Defined(Kind, nameof(Kind));

    /// <summary>What was communicated, checked to be non-empty.</summary>
    public string What { get; } = Checks.Text(What, nameof(What));

    /// <summary>Who classified it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <inheritdoc/>
    public override string ToString() => $"{What}, stated by {StatedBy} to be {Kind} communication";
}

/// <summary>What communicating costs the creature whose turn it is.</summary>
public enum CommunicationCostKind
{
    /// <summary>Free: "Doing so uses neither your action nor your move."</summary>
    Free = 1,

    /// <summary>An action: "Extended communication … requires an action."</summary>
    RequiresAction = 2,
}

/// <summary>What one communication costs, and the rule that says so.</summary>
/// <param name="Communication">The communication, as the caller classified it.</param>
/// <param name="Cost">Free, or an action.</param>
/// <param name="Why">The rule's own reason, in the engine's words.</param>
/// <param name="Requirement">The GM's requirement the cost was worked out under (the assertion <c>gm-requires-action</c>).</param>
/// <param name="Authority">The rule: <c>communication-cost</c>, "Combat / Your Turn / p. 13".</param>
public sealed record CommunicationCost(
    CommunicationOnTurn Communication,
    CommunicationCostKind Cost,
    string Why,
    GmActionRequirement Requirement,
    SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() => $"{Communication}: {Cost} [{Authority.Citation}]";
}
