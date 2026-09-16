using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>single-range</c>'s rule reads.</summary>
    public sealed partial class SingleRangeRequest
    {
        /// <summary>The attack's single range in feet, as the weapon or spell states it. Required.</summary>
        public int? RangeFeet { get; init; }

        /// <summary>The distance to the target, in feet. Required.</summary>
        public int? DistanceFeet { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>single-range</c>: <see cref="TargetingRules.SingleRange"/>, whether the target is within the range.</summary>
        internal static partial Resolution<object> SingleRange(Requests.SingleRangeRequest request) =>
            Answer(Resolution<SingleRangeVerdict>.FromValue(TargetingRules.SingleRange(
                Demand(request.RangeFeet, request.EntryId, nameof(request.RangeFeet)),
                Demand(request.DistanceFeet, request.EntryId, nameof(request.DistanceFeet)))));
    }
}
