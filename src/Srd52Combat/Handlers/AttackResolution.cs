using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>attack-resolution</c>'s rule reads.</summary>
    public sealed partial class AttackResolutionRequest
    {
        /// <summary>Whether the attack roll hit; the roll itself is <c>attack-rolls</c>'. Required, never defaulted.</summary>
        public AttackRollOutcome? Outcome { get; init; }

        /// <summary>Whether the particular attack has rules of its own that specify otherwise. Required, never defaulted.</summary>
        public AttackDamageRules? DamageRules { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>attack-resolution</c>: <see cref="AttackRules.Resolve"/>, whether damage is rolled.</summary>
        internal static partial Resolution<object> AttackResolution(Requests.AttackResolutionRequest request) =>
            Answer(AttackRules.Resolve(
                Demand(request.Outcome, request.EntryId, nameof(request.Outcome)),
                Demand(request.DamageRules, request.EntryId, nameof(request.DamageRules))));
    }
}
