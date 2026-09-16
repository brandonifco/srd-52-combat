using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>grid-entering-square</c>' rule reads.</summary>
    public sealed partial class GridEnteringSquareRequest
    {
        /// <summary>What the caller states about the square being entered: its terrain and its occupant. Required, never defaulted.</summary>
        public SquareStatement? Square { get; init; }

        /// <summary>How the square lies next to your space, orthogonally or diagonally. Required.</summary>
        public SquareAdjacency? Adjacency { get; init; }

        /// <summary>The movement left before entering, in squares. Required.</summary>
        public int? MovementLeftInSquares { get; init; }

        /// <summary>Whether the table plays on a square grid (<c>grid-play</c>). Required, never defaulted.</summary>
        public GridPlayStatement? Play { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>grid-entering-square</c>: <see cref="GridRules.Enter"/>, the cost, or the rule's decline.</summary>
        internal static partial Resolution<object> GridEnteringSquare(Requests.GridEnteringSquareRequest request) =>
            Answer(GridRules.Enter(
                Demand(request.Square, request.EntryId, nameof(request.Square)),
                Demand(request.Adjacency, request.EntryId, nameof(request.Adjacency)),
                Demand(request.MovementLeftInSquares, request.EntryId, nameof(request.MovementLeftInSquares)),
                Demand(request.Play, request.EntryId, nameof(request.Play))));
    }
}
