using RulesKernel.Resolution;
using Srd52Combat.Mounts;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>mount-control-requires-training</c>'s rule reads.</summary>
    public sealed partial class MountControlRequiresTrainingRequest
    {
        /// <summary>Which creature the mount is. Required, never defaulted.</summary>
        public MountCreatureStatement? Creature { get; init; }

        /// <summary>Whether a rider is on it (<c>mounting-cost</c>). Required, never defaulted.</summary>
        public MountStatement? Mount { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>mount-control-requires-training</c>: <see cref="MountRules.Control"/>, whether the mount can be controlled.</summary>
        internal static partial Resolution<object> MountControlRequiresTraining(Requests.MountControlRequiresTrainingRequest request) =>
            Answer(MountRules.Control(
                Demand(request.Creature, request.EntryId, nameof(request.Creature)),
                Demand(request.Mount, request.EntryId, nameof(request.Mount))));
    }
}
