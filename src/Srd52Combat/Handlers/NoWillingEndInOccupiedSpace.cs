using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>no-willing-end-in-occupied-space</c>' rule reads.</summary>
    public sealed partial class NoWillingEndInOccupiedSpaceRequest
    {
        /// <summary>The creature occupying the space the move would end in; null when the caller states none does. Optional.</summary>
        public CreatureInSpace? Occupant { get; init; }

        /// <summary>Whether the creature chose to end its move there, as the caller states it. Required, never inferred.</summary>
        public bool? Willingly { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>no-willing-end-in-occupied-space</c>: <see cref="MovementRules.EndMove"/>, whether the move may end there.</summary>
        internal static partial Resolution<object> NoWillingEndInOccupiedSpace(Requests.NoWillingEndInOccupiedSpaceRequest request) =>
            Answer(Resolution<EndOfMoveRuling>.FromValue(MovementRules.EndMove(
                request.Occupant,
                Demand(request.Willingly, request.EntryId, nameof(request.Willingly)))));
    }
}
