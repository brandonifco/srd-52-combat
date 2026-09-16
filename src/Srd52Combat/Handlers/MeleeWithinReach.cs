using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>melee-within-reach</c>'s rule reads.</summary>
    public sealed partial class MeleeWithinReachRequest
    {
        /// <summary>The distance to the target, in feet. Required.</summary>
        public int? DistanceFeet { get; init; }

        /// <summary>A reach greater than 5 feet, where the attacker's description gives it one.</summary>
        public GreaterReach? Greater { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>melee-within-reach</c>: <see cref="TargetingRules.Melee"/>, whether the target is within reach.</summary>
        internal static partial Resolution<object> MeleeWithinReach(Requests.MeleeWithinReachRequest request) =>
            Answer(Resolution<MeleeTargeting>.FromValue(TargetingRules.Melee(
                Demand(request.DistanceFeet, request.EntryId, nameof(request.DistanceFeet)),
                request.Greater)));
    }
}
