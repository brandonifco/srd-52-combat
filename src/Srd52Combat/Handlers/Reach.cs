using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>reach</c>'s rule reads.</summary>
    public sealed partial class ReachRequest
    {
        /// <summary>
        /// A reach greater than 5 feet, where the creature's description gives it one; null where the
        /// caller states none, which is the 5 feet the rule gives every creature.
        /// </summary>
        public GreaterReach? Greater { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>reach</c>: <see cref="TargetingRules.Reach"/>, the creature's reach in feet.</summary>
        internal static partial Resolution<object> Reach(Requests.ReachRequest request) =>
            Answer(Resolution<CreatureReach>.FromValue(TargetingRules.Reach(request.Greater)));
    }
}
