using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>grid-speed-in-squares</c>' rule reads.</summary>
    public sealed partial class GridSpeedInSquaresRequest
    {
        /// <summary>The creature's Speed in feet; Speed itself is a parameter (<c>speed-and-size-sources</c>). Required.</summary>
        public int? SpeedInFeet { get; init; }

        /// <summary>Whether the table plays on a square grid (<c>grid-play</c>). Required, never defaulted.</summary>
        public GridPlayStatement? Play { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>grid-speed-in-squares</c>: <see cref="GridRules.SpeedInSquares"/>, the Speed in squares, or the rule's decline.</summary>
        internal static partial Resolution<object> GridSpeedInSquares(Requests.GridSpeedInSquaresRequest request) =>
            Answer(GridRules.SpeedInSquares(
                Demand(request.SpeedInFeet, request.EntryId, nameof(request.SpeedInFeet)),
                Demand(request.Play, request.EntryId, nameof(request.Play))));
    }
}
