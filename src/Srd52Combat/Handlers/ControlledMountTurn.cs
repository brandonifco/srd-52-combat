using RulesKernel.Resolution;
using Srd52Combat.Mounts;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>controlled-mount-turn</c>'s rule reads.</summary>
    public sealed partial class ControlledMountTurnRequest
    {
        /// <summary>Which creature the mount is (<c>mount-control-requires-training</c>). Required, never defaulted.</summary>
        public MountCreatureStatement? Creature { get; init; }

        /// <summary>Whether a rider is on it (<c>mounting-cost</c>). Required, never defaulted.</summary>
        public MountStatement? Mount { get; init; }

        /// <summary>An action option whose own rules are asked for; null when the caller asks only what the turn is.</summary>
        public ControlledMountAction? AskingWhatAnActionDoes { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>controlled-mount-turn</c>: <see cref="MountRules.ControlledTurn"/>, the mount's turn, or a decline.</summary>
        internal static partial Resolution<object> ControlledMountTurn(Requests.ControlledMountTurnRequest request) =>
            Answer(MountRules.ControlledTurn(
                Demand(request.Creature, request.EntryId, nameof(request.Creature)),
                Demand(request.Mount, request.EntryId, nameof(request.Mount)),
                request.AskingWhatAnActionDoes));
    }
}
