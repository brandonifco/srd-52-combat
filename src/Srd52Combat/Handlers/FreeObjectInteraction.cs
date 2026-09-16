using RulesKernel.Resolution;
using Srd52Combat.Rules;
using Srd52Combat.Turn;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>free-object-interaction</c>'s rule reads, beside the <c>gm-requires-action</c> assertion.</summary>
    public sealed partial class FreeObjectInteractionRequest
    {
        /// <summary>The turn's interactions with objects and features, in the order they happen. Required.</summary>
        public IReadOnlyList<ObjectInteraction>? Interactions { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary>
        /// <c>free-object-interaction</c>: <see cref="TurnRules.Interactions"/>, what each interaction
        /// costs. The gate <c>gm-requires-action</c> is the caller's assertion, demanded and never
        /// defaulted: whether the GM requires an action decides the answer.
        /// </summary>
        internal static partial Resolution<object> FreeObjectInteraction(Requests.FreeObjectInteractionRequest request) =>
            Answer(TurnRules.Interactions(
                Demand(request.Interactions, request.EntryId, nameof(request.Interactions)),
                GmActionRequirementAsserted(request.Assertions)()));
    }
}
