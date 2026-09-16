using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>grid-corners</c>' rule reads.</summary>
    public sealed partial class GridCornersRequest
    {
        /// <summary>The step being made, orthogonal or diagonal. Required.</summary>
        public SquareAdjacency? Adjacency { get; init; }

        /// <summary>The features the caller states are at the corner the step would cross, and whether each fills its space. Required, never defaulted.</summary>
        public IReadOnlyList<TerrainFeature>? AtTheCorner { get; init; }

        /// <summary>Whether the table plays on a square grid (<c>grid-play</c>). Required, never defaulted.</summary>
        public GridPlayStatement? Play { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>grid-corners</c>: <see cref="GridRules.Corner"/>, whether the step may be made, or the rule's decline.</summary>
        internal static partial Resolution<object> GridCorners(Requests.GridCornersRequest request) =>
            Answer(GridRules.Corner(
                Demand(request.Adjacency, request.EntryId, nameof(request.Adjacency)),
                Demand(request.AtTheCorner, request.EntryId, nameof(request.AtTheCorner)),
                Demand(request.Play, request.EntryId, nameof(request.Play))));
    }
}
