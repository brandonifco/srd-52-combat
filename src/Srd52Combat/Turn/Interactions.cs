using System.Collections.Immutable;
using RulesKernel.Provenance;
using Srd52Combat.Initiative;

namespace Srd52Combat.Turn;

/// <summary>When an object interaction happens: "during either your move or action". There is no default.</summary>
public enum InteractionTiming
{
    /// <summary>During the creature's move ("you could open a door during your move as you stride toward a foe").</summary>
    DuringMove = 1,

    /// <summary>During the creature's action.</summary>
    DuringAction = 2,
}

/// <summary>
/// One interaction with an object or a feature of the environment on a turn, as the caller states
/// it (<c>free-object-interaction</c>, "Combat / Your Turn / p. 13").
/// </summary>
/// <param name="What">The object or feature interacted with, in the caller's words.</param>
/// <param name="During">Whether it happens during the move or during the action.</param>
/// <param name="AlwaysRequiresAction">
/// True when the object is one of the "magic items and other special objects [that] always require
/// an action to use, as stated in their descriptions". The description is the rule, and magic items
/// (p. 204 onward) are outside this engine's slice, so the caller states what the item's own
/// description says; the engine never decides it.
/// </param>
public sealed record ObjectInteraction(string What, InteractionTiming During, bool AlwaysRequiresAction = false)
{
    /// <summary>The object or feature, checked to be non-empty.</summary>
    public string What { get; } = Checks.Text(What, nameof(What));

    /// <summary>The timing, checked to be stated.</summary>
    public InteractionTiming During { get; } = Checks.Defined(During, nameof(During));

    /// <inheritdoc/>
    public override string ToString() => $"interacts with {What} {During}";
}

/// <summary>What an interaction costs the creature whose turn it is.</summary>
public enum InteractionCostKind
{
    /// <summary>Free: neither the action nor the move. The one free interaction of the turn.</summary>
    Free = 1,

    /// <summary>The Utilize action: what a second interaction needs.</summary>
    RequiresUtilizeAction = 2,

    /// <summary>An action, because the GM requires one for the activity, or because the object's own description does.</summary>
    RequiresAction = 3,
}

/// <summary>One interaction and what it costs, with the rule that says so.</summary>
/// <param name="Interaction">The interaction.</param>
/// <param name="Cost">Free, the Utilize action, or an action.</param>
/// <param name="Why">The rule's own reason, in the engine's words.</param>
/// <param name="Authority">The rule: <c>free-object-interaction</c> (p. 13), or <c>gm-requires-action</c> (p. 14) where the GM's determination governs.</param>
public sealed record InteractionCost(ObjectInteraction Interaction, InteractionCostKind Cost, string Why, SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() => $"{Interaction}: {Cost} [{Authority.Citation}]";
}

/// <summary>Every interaction of one turn, in order, and what each costs.</summary>
/// <param name="Costs">One cost per interaction, in the order the interactions were stated.</param>
/// <param name="Requirement">The GM's requirement the costs were worked out under (the assertion <c>gm-requires-action</c>).</param>
/// <param name="Authority">The rule: <c>free-object-interaction</c>, "Combat / Your Turn / p. 13".</param>
public sealed record TurnInteractions(ImmutableArray<InteractionCost> Costs, GmActionRequirement Requirement, SourceLocator Authority)
{
    /// <summary>The interaction that was free, if the turn had one.</summary>
    public InteractionCost? Free => Costs.FirstOrDefault(c => c.Cost == InteractionCostKind.Free);

    /// <inheritdoc/>
    public override string ToString() => string.Join("; ", Costs);
}
