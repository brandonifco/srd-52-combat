using RulesKernel.Resolution;
using Srd52Combat.Rules;
using Srd52Combat.Turn;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>next-round</c>'s rule reads.</summary>
    public sealed partial class NextRoundRequest
    {
        /// <summary>The round whose turns were taken, from 1. Required.</summary>
        public int? Round { get; init; }

        /// <summary>Every combatant, in Initiative order (for example <see cref="Initiative.TurnOrder.TurnsInRound"/>). Required.</summary>
        public IReadOnlyList<string>? Order { get; init; }

        /// <summary>The combatants that have taken their turn in this round. Required; empty when none has.</summary>
        public IReadOnlyList<string>? TurnsTaken { get; init; }

        /// <summary>Whether a side is defeated, as stated. Required, never defaulted.</summary>
        public SideDefeatedStatement? Defeat { get; init; }

        /// <summary>Whether both sides have agreed to end combat, as stated. Required, never defaulted.</summary>
        public SidesAgreementStatement? Agreement { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary>
        /// <c>next-round</c>: <see cref="RoundRules.NextRound"/>, whether another round follows, or
        /// the rule's decline where both sides agree and neither is defeated, which p. 13 and p. 14
        /// answer differently.
        /// </summary>
        internal static partial Resolution<object> NextRound(Requests.NextRoundRequest request) =>
            Answer(RoundRules.NextRound(
                request.Round ?? throw Missing(request.EntryId, nameof(request.Round)),
                Demand(request.Order, request.EntryId, nameof(request.Order)),
                Demand(request.TurnsTaken, request.EntryId, nameof(request.TurnsTaken)),
                Demand(request.Defeat, request.EntryId, nameof(request.Defeat)),
                Demand(request.Agreement, request.EntryId, nameof(request.Agreement))));
    }
}
