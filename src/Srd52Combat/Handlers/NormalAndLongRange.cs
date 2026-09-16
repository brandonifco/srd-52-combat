using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>normal-and-long-range</c>'s rule reads.</summary>
    public sealed partial class NormalAndLongRangeRequest
    {
        /// <summary>The attack's normal and long ranges. Required.</summary>
        public TwoRanges? Ranges { get; init; }

        /// <summary>The distance to the target, in feet.</summary>
        public int DistanceFeet { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>normal-and-long-range</c>: <see cref="AttackRules.Range"/>, the range band and what it does to the roll.</summary>
        internal static partial Resolution<object> NormalAndLongRange(Requests.NormalAndLongRangeRequest request) =>
            Answer(AttackRules.Range(
                Demand(request.Ranges, request.EntryId, nameof(request.Ranges)),
                request.DistanceFeet));
    }
}
