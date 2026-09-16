using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>hidden-attacker-revealed</c>'s rule reads.</summary>
    public sealed partial class HiddenAttackerRevealedRequest
    {
        /// <summary>Whether the attacker is hidden. Required, never defaulted.</summary>
        public HiddenStatement? Hidden { get; init; }

        /// <summary>Whether the attack roll hit; either outcome gives the location away. Required, never defaulted.</summary>
        public AttackRollOutcome? Outcome { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>hidden-attacker-revealed</c>: <see cref="AttackRules.Revealed"/>, whether the location is given away.</summary>
        internal static partial Resolution<object> HiddenAttackerRevealed(Requests.HiddenAttackerRevealedRequest request) =>
            Answer(AttackRules.Revealed(
                Demand(request.Hidden, request.EntryId, nameof(request.Hidden)),
                Demand(request.Outcome, request.EntryId, nameof(request.Outcome))));
    }
}
