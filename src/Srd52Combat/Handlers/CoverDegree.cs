using RulesKernel.Resolution;
using Srd52Combat.Cover;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>cover-degree</c>'s rule reads.</summary>
    public sealed partial class CoverDegreeRequest
    {
        /// <summary>
        /// What the caller states lies between attacker and target, and how much of the target each
        /// covers. Required, never defaulted: an empty list is the caller saying nothing does.
        /// </summary>
        public IReadOnlyList<CoveringObstacle>? Obstacles { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>cover-degree</c>: <see cref="CoverRules.Degree"/>, the degree the Cover table gives, or the rule's decline.</summary>
        internal static partial Resolution<object> CoverDegree(Requests.CoverDegreeRequest request) =>
            Answer(CoverRules.Degree(Demand(request.Obstacles, request.EntryId, nameof(request.Obstacles))));
    }
}
