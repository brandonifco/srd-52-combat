using RulesKernel.Resolution;
using Srd52Combat.Mounts;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>independent-mount</c>'s rule reads.</summary>
    public sealed partial class IndependentMountRequest
    {
        /// <summary>What the mount does with the rider's control. Required, never defaulted.</summary>
        public MountBehaviourStatement? Behaviour { get; init; }

        /// <summary>Whether a rider is on it (<c>mounting-cost</c>). Required, never defaulted.</summary>
        public MountStatement? Mount { get; init; }

        /// <summary>True when the caller asks which moves and actions the mount takes.</summary>
        public bool AskingWhatItDoes { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>independent-mount</c>: <see cref="MountRules.Independent"/>, what the rule says of an independent mount, or a decline.</summary>
        internal static partial Resolution<object> IndependentMount(Requests.IndependentMountRequest request) =>
            Answer(MountRules.Independent(
                Demand(request.Behaviour, request.EntryId, nameof(request.Behaviour)),
                Demand(request.Mount, request.EntryId, nameof(request.Mount)),
                request.AskingWhatItDoes));
    }
}
