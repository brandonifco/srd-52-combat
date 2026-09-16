using RulesKernel.Resolution;
using Srd52Combat.Mounts;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>mounting-cost</c>'s rule reads.</summary>
    public sealed partial class MountingCostRequest
    {
        /// <summary>Mounting or dismounting. Required.</summary>
        public MountAction? Action { get; init; }

        /// <summary>The rider's Speed in feet (<c>speed-and-size-sources</c>). Required.</summary>
        public int? SpeedInFeet { get; init; }

        /// <summary>The movement left in this move, in feet (<c>movement-deduction</c>). Required.</summary>
        public int? MovementLeftFeet { get; init; }

        /// <summary>How far away the creature is, in feet; read only when mounting. Required.</summary>
        public int? DistanceToMountFeet { get; init; }

        /// <summary>Whether the creature serves as a mount (<c>mount-eligibility</c>). Required, never defaulted.</summary>
        public MountStatement? Mount { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>mounting-cost</c>: <see cref="MountRules.Mounting"/>, what it costs and whether it is done.</summary>
        internal static partial Resolution<object> MountingCost(Requests.MountingCostRequest request) =>
            Answer(MountRules.Mounting(
                Demand(request.Action, request.EntryId, nameof(request.Action)),
                Demand(request.SpeedInFeet, request.EntryId, nameof(request.SpeedInFeet)),
                Demand(request.MovementLeftFeet, request.EntryId, nameof(request.MovementLeftFeet)),
                Demand(request.DistanceToMountFeet, request.EntryId, nameof(request.DistanceToMountFeet)),
                Demand(request.Mount, request.EntryId, nameof(request.Mount))));
    }
}
