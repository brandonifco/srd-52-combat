using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>moving-through-creatures</c>' rule reads.</summary>
    public sealed partial class MovingThroughCreaturesRequest
    {
        /// <summary>Your size category, as the caller states it. Required, never inferred.</summary>
        public CreatureSize? YourSize { get; init; }

        /// <summary>The creature whose space you would pass through, as the caller states it. Required, never defaulted.</summary>
        public CreatureInSpace? Other { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>moving-through-creatures</c>: <see cref="MovementRules.PassThrough"/>, that you may pass through, or the rule's decline.</summary>
        internal static partial Resolution<object> MovingThroughCreatures(Requests.MovingThroughCreaturesRequest request) =>
            Answer(MovementRules.PassThrough(
                Demand(request.YourSize, request.EntryId, nameof(request.YourSize)),
                Demand(request.Other, request.EntryId, nameof(request.Other))));
    }
}
