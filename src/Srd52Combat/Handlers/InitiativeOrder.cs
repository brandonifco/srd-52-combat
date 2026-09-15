using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>initiative-order</c>'s rule reads. Tie breaks are the <c>initiative-ties</c> assertion, demanded only when there is a tie.</summary>
    public sealed partial class InitiativeOrderRequest
    {
        /// <summary>Every combatant's Initiative (for example <see cref="InitiativeRolls.Counts"/>). Required.</summary>
        public IReadOnlyList<InitiativeCount>? Counts { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>initiative-order</c>: <see cref="InitiativeRules.Order"/>, the order, or the rule's decline.</summary>
        internal static partial Resolution<object> InitiativeOrder(Requests.InitiativeOrderRequest request) =>
            Answer(InitiativeRules.Order(
                Demand(request.Counts, request.EntryId, nameof(request.Counts)),
                TieBreaksAsserted(request.Assertions)));
    }
}
