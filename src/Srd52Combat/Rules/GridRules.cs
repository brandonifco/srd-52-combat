using System.Collections.Immutable;
using RulesKernel.Resolution;
using Srd52Combat.Movement;

namespace Srd52Combat.Rules;

/// <summary>
/// The grid rules, "Combat / Playing on a Grid / p. 13": the square's size
/// (<c>grid-square-size</c>), Speed in squares (<c>grid-speed-in-squares</c>), what entering a square
/// costs (<c>grid-entering-square</c>) and corners (<c>grid-corners</c>).
/// </summary>
/// <remarks>
/// <para>
/// These rules are an optional variant: "If you play using a square grid and miniatures or other
/// tokens, follow these rules" (<c>grid-play</c>), which each of them names in <c>enabledBy</c>.
/// Whether the table uses a grid gets no entry of its own, so every rule here demands the caller's
/// <see cref="GridPlayStatement"/> and, where the table does not play on a grid, declines
/// <see cref="UnresolvedReason.OutsideCurrentScope"/> citing <c>grid-play</c>: off the grid the
/// default rules govern and distances are in feet, and the engine answers no grid question.
/// </para>
/// <para>
/// Every decline names its entry in <see cref="UnresolvedResult.Attempted"/>, because three of these
/// entries cite the same page.
/// </para>
/// </remarks>
public static class GridRules
{
    /// <summary>
    /// <c>grid-square-size</c>: "Squares. Each square represents 5 feet."
    /// </summary>
    /// <param name="play">Whether the table plays on a square grid, as the caller states it.</param>
    /// <returns>The square's size, or the decline citing <c>grid-play</c> off the grid.</returns>
    public static Resolution<GridSquare> Square(GridPlayStatement play)
    {
        ArgumentNullException.ThrowIfNull(play);
        return OffTheGrid(MapEntries.GridSquareSize, play) is { } declined
            ? Resolution<GridSquare>.FromUnresolved(declined)
            : Resolution<GridSquare>.FromValue(new GridSquare(FeetPerSquare, play, MapEntries.GridSquareSize.Locator));
    }

    /// <summary>
    /// <c>grid-speed-in-squares</c>: "You can translate your Speed into squares by dividing it by 5."
    /// </summary>
    /// <remarks>
    /// The divisor is <c>grid-square-size</c>'s 5 feet, read through <see cref="Square"/> rather than
    /// written again here. A Speed that is not a whole number of squares leaves a fraction, and the
    /// general rule rounds it down (<c>round-down</c>, "Playing the Game / Round Down / p. 5", which
    /// the map's note for this entry cites): a Speed of 12 feet is 2 squares.
    /// </remarks>
    /// <param name="speedInFeet">The creature's Speed in feet, as the caller states it. Speed itself is a parameter (<c>speed-and-size-sources</c>).</param>
    /// <param name="play">Whether the table plays on a square grid, as the caller states it.</param>
    /// <returns>The Speed in squares, or the decline citing <c>grid-play</c> off the grid.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="speedInFeet"/> is negative.</exception>
    public static Resolution<SpeedInSquares> SpeedInSquares(int speedInFeet, GridPlayStatement play)
    {
        ArgumentNullException.ThrowIfNull(play);
        ArgumentOutOfRangeException.ThrowIfNegative(speedInFeet);
        if (OffTheGrid(MapEntries.GridSpeedInSquares, play) is { } declined)
        {
            return Resolution<SpeedInSquares>.FromUnresolved(declined);
        }

        return Square(play).Match(
            square => Resolution<SpeedInSquares>.FromValue(new SpeedInSquares(
                speedInFeet,
                speedInFeet / square.Feet,
                square,
                MapEntries.GridSpeedInSquares.Locator)),
            Resolution<SpeedInSquares>.FromUnresolved);
    }

    /// <summary>
    /// <c>grid-entering-square</c>: "It costs 1 square of movement to enter an unoccupied square
    /// that's adjacent to your space (orthogonally or diagonally adjacent). A square of Difficult
    /// Terrain costs 2 squares to enter."
    /// </summary>
    /// <remarks>
    /// A diagonal step costs what an orthogonal one costs: the rule prices both the same. A square
    /// another creature occupies is Difficult Terrain unless that creature is Tiny or your ally
    /// (<c>creature-space-difficult-terrain</c>), so entering it costs 2. A square occupied by a
    /// creature whose space is <em>not</em> Difficult Terrain for you is the entry's unresolved
    /// question -- the rule prices only an <em>unoccupied</em> square at 1 -- so that case declines
    /// <see cref="UnresolvedReason.RequiresInterpretation"/> citing this entry rather than choosing
    /// a reading.
    /// </remarks>
    /// <param name="square">What the caller states about the square being entered.</param>
    /// <param name="adjacency">How it lies next to your space.</param>
    /// <param name="movementLeftInSquares">The movement left before entering, in squares.</param>
    /// <param name="play">Whether the table plays on a square grid, as the caller states it.</param>
    /// <returns>The cost and whether it can be paid; or a decline.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="movementLeftInSquares"/> is negative, or <paramref name="adjacency"/> is not stated.</exception>
    public static Resolution<SquareEntry> Enter(
        SquareStatement square,
        SquareAdjacency adjacency,
        int movementLeftInSquares,
        GridPlayStatement play)
    {
        ArgumentNullException.ThrowIfNull(square);
        ArgumentNullException.ThrowIfNull(play);
        ArgumentOutOfRangeException.ThrowIfNegative(movementLeftInSquares);
        if (!Enum.IsDefined(adjacency))
        {
            throw new ArgumentOutOfRangeException(nameof(adjacency), adjacency, "a square entered is orthogonally or diagonally adjacent");
        }

        if (OffTheGrid(MapEntries.GridEnteringSquare, play) is { } declined)
        {
            return Resolution<SquareEntry>.FromUnresolved(declined);
        }

        bool difficultTerrain = square.DifficultTerrain;
        if (square.Occupant is { } occupant)
        {
            var terrain = MovementRules.SpaceOf(occupant);
            difficultTerrain = difficultTerrain || terrain.DifficultTerrainForYou;
            if (!difficultTerrain)
            {
                return Resolution<SquareEntry>.FromUnresolved(new UnresolvedResult(
                    UnresolvedReason.RequiresInterpretation,
                    $"{Cited.Attempting(MapEntries.GridEnteringSquare)} while {square}: the rule prices an unoccupied square at 1 "
                    + $"and a square of Difficult Terrain at 2, and {terrain}, so it prices neither",
                    MapEntries.GridEnteringSquare.Locator));
            }
        }

        int cost = difficultTerrain ? DifficultTerrainCost : PlainCost;
        return Resolution<SquareEntry>.FromValue(
            new SquareEntry(cost, movementLeftInSquares, adjacency, square, play, MapEntries.GridEnteringSquare.Locator));
    }

    /// <summary>
    /// <c>grid-corners</c>: "Diagonal movement can't cross the corner of a wall, a large tree, or
    /// another terrain feature that fills its space."
    /// </summary>
    /// <remarks>
    /// The rule speaks of diagonal movement only, so an orthogonal step is not stopped by it, and a
    /// feature at the corner that does not fill its space stops nothing. Whether a feature fills its
    /// space is the caller's statement.
    /// </remarks>
    /// <param name="adjacency">The step being made.</param>
    /// <param name="atTheCorner">The features the caller states are at the corner the step would cross.</param>
    /// <param name="play">Whether the table plays on a square grid, as the caller states it.</param>
    /// <returns>Whether the step may be made, or the decline citing <c>grid-play</c> off the grid.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="adjacency"/> is not stated.</exception>
    public static Resolution<CornerCrossing> Corner(
        SquareAdjacency adjacency,
        IReadOnlyList<TerrainFeature> atTheCorner,
        GridPlayStatement play)
    {
        ArgumentNullException.ThrowIfNull(atTheCorner);
        ArgumentNullException.ThrowIfNull(play);
        if (atTheCorner.Any(f => f is null))
        {
            throw new ArgumentException("no feature at the corner is null", nameof(atTheCorner));
        }

        if (!Enum.IsDefined(adjacency))
        {
            throw new ArgumentOutOfRangeException(nameof(adjacency), adjacency, "a step is orthogonal or diagonal");
        }

        if (OffTheGrid(MapEntries.GridCorners, play) is { } declined)
        {
            return Resolution<CornerCrossing>.FromUnresolved(declined);
        }

        var features = atTheCorner.ToImmutableArray();
        var blocking = adjacency == SquareAdjacency.Diagonal
            ? features.FirstOrDefault(f => f.FillsItsSpace)
            : null;
        return Resolution<CornerCrossing>.FromValue(new CornerCrossing(
            blocking is null,
            adjacency,
            blocking,
            features,
            play,
            MapEntries.GridCorners.Locator));
    }

    /// <summary>The feet one square represents, as <c>grid-square-size</c> states it.</summary>
    private const int FeetPerSquare = 5;

    /// <summary>What entering an unoccupied adjacent square costs, in squares.</summary>
    private const int PlainCost = 1;

    /// <summary>What entering a square of Difficult Terrain costs, in squares.</summary>
    private const int DifficultTerrainCost = 2;

    /// <summary>
    /// The decline a grid rule gives where the table does not play on a grid: the grid rules are the
    /// variant <c>grid-play</c> opens, and nothing opens them off the grid.
    /// </summary>
    private static UnresolvedResult? OffTheGrid(MapEntry entry, GridPlayStatement play) =>
        play.OnASquareGrid
            ? null
            : new UnresolvedResult(
                UnresolvedReason.OutsideCurrentScope,
                $"{Cited.Attempting(entry)} while {play}: the grid rules apply only to a table that plays on a square grid "
                + $"('{MapEntries.GridPlay.Id}'), and off the grid distances are in feet",
                MapEntries.GridPlay.Locator);
}
