using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>opportunity-attack</c>'s rule reads.</summary>
    public sealed partial class OpportunityAttackRequest
    {
        /// <summary>Whether the attacker can see the creature. Required, never defaulted.</summary>
        public TargetVisibilityStatement? Sight { get; init; }

        /// <summary>How the creature leaves the attacker's reach. Required, never defaulted.</summary>
        public LeavingReach? Leaving { get; init; }

        /// <summary>Whether the attacker still has its Reaction. Required, never defaulted.</summary>
        public ReactionAvailability? Reaction { get; init; }

        /// <summary>Whether the attacker has the Incapacitated condition. Required, never defaulted.</summary>
        public IncapacitatedStatement? Incapacitated { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>opportunity-attack</c>: <see cref="OpportunityAttackRules.Make"/>, the offer, or the rule's decline.</summary>
        internal static partial Resolution<object> OpportunityAttack(Requests.OpportunityAttackRequest request) =>
            Answer(OpportunityAttackRules.Make(
                Demand(request.Sight, request.EntryId, nameof(request.Sight)),
                Demand(request.Leaving, request.EntryId, nameof(request.Leaving)),
                Demand(request.Reaction, request.EntryId, nameof(request.Reaction)),
                Demand(request.Incapacitated, request.EntryId, nameof(request.Incapacitated))));
    }
}
