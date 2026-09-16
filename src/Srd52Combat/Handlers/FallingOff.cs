using RulesKernel.Resolution;
using Srd52Combat.Mounts;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>falling-off</c>'s rule reads.</summary>
    public sealed partial class FallingOffRequest
    {
        /// <summary>Which of the rule's three cases is in play. Required.</summary>
        public FallTrigger? Trigger { get; init; }

        /// <summary>
        /// What the DC 10 Dexterity saving throw did, as the caller states it (<c>saving-throws</c>
        /// is outside this engine's extent); null when the caller asks only what the rule demands.
        /// </summary>
        public SaveOutcome? Outcome { get; init; }

        /// <summary>Whether a rider is on the mount (<c>mounting-cost</c>). Required, never defaulted.</summary>
        public MountStatement? Mount { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>falling-off</c>: <see cref="MountRules.FallingOff"/>, the save the rule demands and what it did, or a decline.</summary>
        internal static partial Resolution<object> FallingOff(Requests.FallingOffRequest request) =>
            Answer(MountRules.FallingOff(
                Demand(request.Trigger, request.EntryId, nameof(request.Trigger)),
                request.Outcome,
                Demand(request.Mount, request.EntryId, nameof(request.Mount))));
    }
}
