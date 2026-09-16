using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>ranged-in-close-combat</c>'s rule reads.</summary>
    public sealed partial class RangedInCloseCombatRequest
    {
        /// <summary>Every enemy within 5 feet of the attacker. Required, never defaulted.</summary>
        public EnemiesWithinFiveFeet? Enemies { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>ranged-in-close-combat</c>: <see cref="AttackRules.RangedInCloseCombat"/>, the determination.</summary>
        internal static partial Resolution<object> RangedInCloseCombat(Requests.RangedInCloseCombatRequest request) =>
            Answer(AttackRules.RangedInCloseCombat(Demand(request.Enemies, request.EntryId, nameof(request.Enemies))));
    }
}
