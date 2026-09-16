using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>wrong-location-misses</c>' rule reads.</summary>
    public sealed partial class WrongLocationMissesRequest
    {
        /// <summary>The location the attacker targeted, and whether the target is in it. Required, never defaulted.</summary>
        public TargetLocationStatement? Location { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>wrong-location-misses</c>: <see cref="AttackRules.WrongLocation"/>, whether the attack misses.</summary>
        internal static partial Resolution<object> WrongLocationMisses(Requests.WrongLocationMissesRequest request) =>
            Answer(AttackRules.WrongLocation(Demand(request.Location, request.EntryId, nameof(request.Location))));
    }
}
