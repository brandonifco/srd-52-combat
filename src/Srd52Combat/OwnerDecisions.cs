using System.Collections.Immutable;

namespace Srd52Combat;

/// <summary>
/// A decision of this engine's, taken by its owner, where the map records no open question for
/// rules-factory decision 0027 to carry: the entry is <c>clarity: clear</c>, so it has no
/// <c>ambiguity.question</c> to quote a span of, and the overlay would refuse a <c>rulings</c> item on
/// it. It is deliberately not an <see cref="OwnerRuling"/>: a different claim, with a different
/// carrier, so nothing mistakes one for the other, and neither for the corpus. A result that relies on
/// one names it, on the same terms 0027 § 4 sets for a ruling.
/// </summary>
/// <param name="Id">A stable id, which is not a ruling id: it carries no slash.</param>
/// <param name="EntryId">The map entry whose rule the decision settles the reach of.</param>
/// <param name="Answer">The decision, stated briefly.</param>
/// <param name="DecidedBy">Who decided.</param>
/// <param name="DecidedOn">When.</param>
/// <param name="Record">The engine's decision record that holds it, relative to the engine root.</param>
public sealed record OwnerDecision(string Id, string EntryId, string Answer, string DecidedBy, DateOnly DecidedOn, string Record)
{
    /// <inheritdoc/>
    public override string ToString() =>
        $"{Id} on '{EntryId}', this engine's decision and not the corpus: {Answer} (decided by {DecidedBy} on {DecidedOn:yyyy-MM-dd}, {Record})";
}

/// <summary>Every such decision this engine holds, in the order it took them.</summary>
public static class OwnerDecisions
{
    /// <summary>
    /// <c>opportunity-attack-avoidance</c>: the slice says "You can avoid provoking an Opportunity
    /// Attack by taking the Disengage action" and states no limit; the Rules Glossary's
    /// <c>disengage-action</c> (p. 181, <c>scope: out</c>, which the entry names in <c>dependsOn</c>)
    /// limits the protection to "your movement" and to "the rest of the current turn". Brandon decided
    /// the engine follows the glossary (MAP-FINDINGS finding 11, <c>docs/decisions/0007</c>).
    /// </summary>
    public static OwnerDecision DisengageCoversYourOwnMovementThisTurn { get; } = new(
        "disengage-protection-follows-the-glossary",
        "opportunity-attack-avoidance",
        "The Disengage action's protection covers your own movement for the rest of your turn, following disengage-action (p. 181), and no longer.",
        "Brandon",
        new DateOnly(2026, 9, 16),
        "docs/decisions/0007-brandons-rulings-on-six-open-questions-are-ruleset-version-seven.md");

    /// <summary>Every decision, in the order they were taken.</summary>
    public static ImmutableArray<OwnerDecision> All { get; } = [DisengageCoversYourOwnMovementThisTurn];

    /// <summary>No decision was relied on.</summary>
    public static ImmutableArray<OwnerDecision> None { get; } = [];

    /// <summary><paramref name="decisions"/> without repeats, in <see cref="All"/>'s order.</summary>
    /// <param name="decisions">The decisions the result relied on, in any order and with any repeats.</param>
    /// <returns>Them, once each, in this engine's order.</returns>
    public static ImmutableArray<OwnerDecision> InOrder(IEnumerable<OwnerDecision> decisions)
    {
        ArgumentNullException.ThrowIfNull(decisions);
        var relied = decisions.ToHashSet();
        return [.. All.Where(relied.Contains)];
    }
}
