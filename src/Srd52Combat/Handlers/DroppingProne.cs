using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>dropping-prone</c>' rule reads.</summary>
    public sealed partial class DroppingProneRequest
    {
        /// <summary>The creature's Speed in feet; Speed itself is a parameter (<c>speed-and-size-sources</c>). Required.</summary>
        public int? SpeedInFeet { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>dropping-prone</c>: <see cref="MovementRules.DropProne"/>, whether the creature can drop Prone.</summary>
        internal static partial Resolution<object> DroppingProne(Requests.DroppingProneRequest request) =>
            Answer(Resolution<DroppingProneRuling>.FromValue(
                MovementRules.DropProne(Demand(request.SpeedInFeet, request.EntryId, nameof(request.SpeedInFeet)))));
    }
}
