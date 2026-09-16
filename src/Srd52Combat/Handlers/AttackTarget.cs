using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>attack-target</c>'s rule reads.</summary>
    public sealed partial class AttackTargetRequest
    {
        /// <summary>A creature, an object, or a location. Required.</summary>
        public TargetKind Kind { get; init; }

        /// <summary>The target's name or handle. Required.</summary>
        public string? Target { get; init; }

        /// <summary>Which range rule the attack is measured by. Required.</summary>
        public AttackRangeKind RangeKind { get; init; }

        /// <summary>The attack's two ranges; required for a ranged attack that has them.</summary>
        public TwoRanges? Ranges { get; init; }

        /// <summary>The distance to the target, in feet.</summary>
        public int DistanceFeet { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>attack-target</c>: <see cref="AttackRules.Target"/>, the chosen target, or the rule's decline.</summary>
        internal static partial Resolution<object> AttackTarget(Requests.AttackTargetRequest request) =>
            Answer(AttackRules.Target(
                request.Kind,
                Demand(request.Target, request.EntryId, nameof(request.Target)),
                request.RangeKind,
                request.Ranges,
                request.DistanceFeet));
    }
}
