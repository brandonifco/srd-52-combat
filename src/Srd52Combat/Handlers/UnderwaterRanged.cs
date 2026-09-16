using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Rules;
using Srd52Combat.Underwater;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>underwater-ranged</c>'s rule reads.</summary>
    public sealed partial class UnderwaterRangedRequest
    {
        /// <summary>Whether the fight is underwater. Required, never defaulted.</summary>
        public UnderwaterStatement? Where { get; init; }

        /// <summary>The weapon's normal and long ranges (<c>normal-and-long-range</c>). Required.</summary>
        public TwoRanges? Ranges { get; init; }

        /// <summary>The distance to the target, in feet. Required.</summary>
        public int? DistanceFeet { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>underwater-ranged</c>: <see cref="UnderwaterRules.Ranged"/>, the automatic miss or the Disadvantage.</summary>
        internal static partial Resolution<object> UnderwaterRanged(Requests.UnderwaterRangedRequest request) =>
            Answer(UnderwaterRules.Ranged(
                Demand(request.Where, request.EntryId, nameof(request.Where)),
                Demand(request.Ranges, request.EntryId, nameof(request.Ranges)),
                Demand(request.DistanceFeet, request.EntryId, nameof(request.DistanceFeet))));
    }
}
