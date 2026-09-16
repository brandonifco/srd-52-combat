using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>unseen-target-disadvantage</c>'s rule reads.</summary>
    public sealed partial class UnseenTargetDisadvantageRequest
    {
        /// <summary>Whether the attacker can see the target. Required, never defaulted.</summary>
        public TargetVisibilityStatement? Visibility { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>unseen-target-disadvantage</c>: <see cref="AttackRules.UnseenTarget"/>, the determination.</summary>
        internal static partial Resolution<object> UnseenTargetDisadvantage(Requests.UnseenTargetDisadvantageRequest request) =>
            Answer(AttackRules.UnseenTarget(Demand(request.Visibility, request.EntryId, nameof(request.Visibility))));
    }
}
