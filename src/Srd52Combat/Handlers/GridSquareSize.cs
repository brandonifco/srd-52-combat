using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>grid-square-size</c>' rule reads.</summary>
    public sealed partial class GridSquareSizeRequest
    {
        /// <summary>Whether the table plays on a square grid (<c>grid-play</c>). Required, never defaulted.</summary>
        public GridPlayStatement? Play { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>grid-square-size</c>: <see cref="GridRules.Square"/>, the square's size, or the rule's decline.</summary>
        internal static partial Resolution<object> GridSquareSize(Requests.GridSquareSizeRequest request) =>
            Answer(GridRules.Square(Demand(request.Play, request.EntryId, nameof(request.Play))));
    }
}
