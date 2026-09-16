using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Rules;
using Srd52Combat.Turn;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>surprise-disadvantage</c>'s rule reads.</summary>
    public sealed partial class SurpriseDisadvantageRequest
    {
        /// <summary>Whether the combatant is surprised by combat starting, as stated. Required, never defaulted.</summary>
        public SurprisedStatement? Surprised { get; init; }

        /// <summary>Whether the GM uses Initiative scores instead of rolling. Required, never defaulted.</summary>
        public InitiativeScoreOptionStatement? ScoreOption { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>surprise-disadvantage</c>: <see cref="SurpriseRules.OnInitiative"/>, the Disadvantage surprise gives, or the rule's decline.</summary>
        internal static partial Resolution<object> SurpriseDisadvantage(Requests.SurpriseDisadvantageRequest request) =>
            Answer(SurpriseRules.OnInitiative(
                Demand(request.Surprised, request.EntryId, nameof(request.Surprised)),
                Demand(request.ScoreOption, request.EntryId, nameof(request.ScoreOption))));
    }
}
