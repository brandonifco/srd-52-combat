using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>unseen-attacker-advantage</c>'s rule reads.</summary>
    public sealed partial class UnseenAttackerAdvantageRequest
    {
        /// <summary>Whether the creature can see the attacker. Required, never defaulted.</summary>
        public AttackerSeenStatement? Seen { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>unseen-attacker-advantage</c>: <see cref="AttackRules.UnseenAttacker"/>, the determination.</summary>
        internal static partial Resolution<object> UnseenAttackerAdvantage(Requests.UnseenAttackerAdvantageRequest request) =>
            Answer(AttackRules.UnseenAttacker(Demand(request.Seen, request.EntryId, nameof(request.Seen))));
    }
}
