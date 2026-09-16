using RulesKernel.Resolution;
using Srd52Combat.Rules;
using Srd52Combat.Underwater;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>underwater-melee</c>'s rule reads.</summary>
    public sealed partial class UnderwaterMeleeRequest
    {
        /// <summary>Whether the fight is underwater. Required, never defaulted.</summary>
        public UnderwaterStatement? Where { get; init; }

        /// <summary>Whether the attacker has a Swim Speed. Required, never defaulted.</summary>
        public SwimSpeedStatement? SwimSpeed { get; init; }

        /// <summary>The weapon and whether it deals Piercing damage. Required, never defaulted.</summary>
        public WeaponStatement? Weapon { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>underwater-melee</c>: <see cref="UnderwaterRules.Melee"/>, what the rule does to the attack roll.</summary>
        internal static partial Resolution<object> UnderwaterMelee(Requests.UnderwaterMeleeRequest request) =>
            Answer(UnderwaterRules.Melee(
                Demand(request.Where, request.EntryId, nameof(request.Where)),
                Demand(request.SwimSpeed, request.EntryId, nameof(request.SwimSpeed)),
                Demand(request.Weapon, request.EntryId, nameof(request.Weapon))));
    }
}
