using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>opportunity-attack-avoidance</c>'s rule reads.</summary>
    public sealed partial class OpportunityAttackAvoidanceRequest
    {
        /// <summary>How the creature left reach, and whether it took the Disengage action. Required, never defaulted.</summary>
        public LeavingReach? Leaving { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>opportunity-attack-avoidance</c>: <see cref="OpportunityAttackRules.Avoidance"/>, whether the movement provokes.</summary>
        internal static partial Resolution<object> OpportunityAttackAvoidance(Requests.OpportunityAttackAvoidanceRequest request) =>
            Answer(OpportunityAttackRules.Avoidance(Demand(request.Leaving, request.EntryId, nameof(request.Leaving))));
    }
}
