using System.Collections.Immutable;
using RulesKernel.Provenance;
using Srd52Combat.Initiative;

namespace Srd52Combat.Movement;

/// <summary>
/// Whether the table plays on a square grid, as the caller states it. "If you play using a square
/// grid and miniatures or other tokens, follow these rules" (<c>grid-play</c>, "Combat / Playing on
/// a Grid / p. 13"), which every grid entry names in <c>enabledBy</c>. Whether the table uses one is
/// a caller-supplied parameter and gets no entry of its own, so the engine demands the statement and
/// never assumes a grid: off the grid the default rules govern, and distances are in feet.
/// </summary>
/// <param name="OnASquareGrid">True when the table plays on a square grid with miniatures or tokens.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record GridPlayStatement(bool OnASquareGrid, string StatedBy)
{
    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The table plays on a square grid.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static GridPlayStatement OnAGrid(string statedBy) => new(OnASquareGrid: true, statedBy);

    /// <summary>The table does not play on a square grid.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static GridPlayStatement NotOnAGrid(string statedBy) => new(OnASquareGrid: false, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        OnASquareGrid
            ? $"the table plays on a square grid, as stated by {StatedBy}"
            : $"the table does not play on a square grid, as stated by {StatedBy}";
}

/// <summary>
/// How a square being entered lies next to your space: "adjacent to your space (orthogonally or
/// diagonally adjacent)" (<c>grid-entering-square</c>). There is no default: <c>default</c> is refused.
/// </summary>
public enum SquareAdjacency
{
    /// <summary>Orthogonally adjacent: a step along a row or a column.</summary>
    Orthogonal = 1,

    /// <summary>Diagonally adjacent: a step across a corner.</summary>
    Diagonal = 2,
}

/// <summary>
/// What the caller states about the square being entered: whether it is Difficult Terrain, and the
/// creature occupying it, if any. What counts as Difficult Terrain is <c>difficult-terrain</c>, whose
/// question the map leaves unresolved, so which squares hold it is a fact the caller supplies and the
/// engine never classifies.
/// </summary>
/// <param name="DifficultTerrain">True when the caller states the square's terrain is Difficult Terrain.</param>
/// <param name="Occupant">The creature occupying the square, or null when the caller states it is unoccupied.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record SquareStatement(bool DifficultTerrain, CreatureInSpace? Occupant, string StatedBy)
{
    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>An unoccupied square whose terrain is not Difficult Terrain.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static SquareStatement Unoccupied(string statedBy) => new(DifficultTerrain: false, Occupant: null, statedBy);

    /// <summary>An unoccupied square of Difficult Terrain.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static SquareStatement UnoccupiedDifficultTerrain(string statedBy) =>
        new(DifficultTerrain: true, Occupant: null, statedBy);

    /// <summary>A square occupied by <paramref name="occupant"/>, whose own terrain is not Difficult Terrain.</summary>
    /// <param name="occupant">The creature occupying it.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static SquareStatement OccupiedBy(CreatureInSpace occupant, string statedBy) =>
        new(DifficultTerrain: false, occupant, statedBy);

    /// <inheritdoc/>
    public override string ToString()
    {
        string terrain = DifficultTerrain ? "Difficult Terrain" : "not Difficult Terrain";
        string occupancy = Occupant is null ? "unoccupied" : $"occupied by {Occupant.Id}";
        return $"the square is {terrain} and {occupancy}, as stated by {StatedBy}";
    }
}

/// <summary>
/// A terrain feature at a corner a diagonal step would cross, as the caller states it: "a wall, a
/// large tree, or another terrain feature that fills its space" (<c>grid-corners</c>). Whether a
/// feature fills its space is a fact of the table's map the caller supplies, not a blank in the rule.
/// </summary>
/// <param name="Name">What the feature is, in the caller's words.</param>
/// <param name="FillsItsSpace">True when the caller states the feature fills its space.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record TerrainFeature(string Name, bool FillsItsSpace, string StatedBy)
{
    /// <summary>The name, checked to be non-empty.</summary>
    public string Name { get; } = Checks.Text(Name, nameof(Name));

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>A feature the caller states fills its space.</summary>
    /// <param name="name">What the feature is.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static TerrainFeature Filling(string name, string statedBy) => new(name, FillsItsSpace: true, statedBy);

    /// <summary>A feature the caller states does not fill its space.</summary>
    /// <param name="name">What the feature is.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static TerrainFeature NotFilling(string name, string statedBy) => new(name, FillsItsSpace: false, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {(FillsItsSpace ? "fills" : "does not fill")} its space, as stated by {StatedBy}";
}

/// <summary>The square's size, the value <c>grid-square-size</c> states: "Each square represents 5 feet."</summary>
/// <param name="Feet">The feet one square represents.</param>
/// <param name="Play">The grid-play statement it was resolved under.</param>
/// <param name="Authority">The rule: <c>grid-square-size</c>, "Combat / Playing on a Grid / p. 13".</param>
public sealed record GridSquare(int Feet, GridPlayStatement Play, SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() => $"each square represents {Feet} feet [{Authority}]";
}

/// <summary>
/// A Speed translated into squares (<c>grid-speed-in-squares</c>): "You can translate your Speed
/// into squares by dividing it by 5."
/// </summary>
/// <param name="SpeedInFeet">The Speed the caller stated, in feet.</param>
/// <param name="Squares">The Speed in squares, the fraction rounded down.</param>
/// <param name="Square">The square size it was divided by.</param>
/// <param name="Authority">The rule: <c>grid-speed-in-squares</c>, "Combat / Playing on a Grid / p. 13".</param>
public sealed record SpeedInSquares(int SpeedInFeet, int Squares, GridSquare Square, SourceLocator Authority)
{
    /// <summary>True when the Speed is not a whole number of squares and the fraction was rounded down.</summary>
    public bool RoundedDown => SpeedInFeet % Square.Feet != 0;

    /// <summary>The grid-play statement it was resolved under.</summary>
    public GridPlayStatement Play => Square.Play;

    /// <inheritdoc/>
    public override string ToString() =>
        $"a Speed of {SpeedInFeet} feet translates into {Squares} squares"
        + (RoundedDown ? " (rounded down)" : string.Empty)
        + $" [{Authority}]";
}

/// <summary>
/// What entering one square costs, and whether the movement left pays for it
/// (<c>grid-entering-square</c>).
/// </summary>
/// <param name="Cost">The cost in squares: 1, or 2 for a square of Difficult Terrain.</param>
/// <param name="MovementLeftInSquares">The movement left before entering, as the caller stated it.</param>
/// <param name="Adjacency">How the square lies next to your space; a diagonal step costs the same as an orthogonal one.</param>
/// <param name="Square">What the caller stated about the square.</param>
/// <param name="Play">The grid-play statement it was resolved under.</param>
/// <param name="Authority">The rule: <c>grid-entering-square</c>, "Combat / Playing on a Grid / p. 13".</param>
public sealed record SquareEntry(
    int Cost,
    int MovementLeftInSquares,
    SquareAdjacency Adjacency,
    SquareStatement Square,
    GridPlayStatement Play,
    SourceLocator Authority)
{
    /// <summary>True when there is enough movement left to pay for entering.</summary>
    public bool EnoughMovementLeft => MovementLeftInSquares >= Cost;

    /// <inheritdoc/>
    public override string ToString() =>
        $"entering the {Adjacency.ToString().ToLowerInvariant()}ly adjacent square costs {Cost} square(s) of movement, "
        + $"and {MovementLeftInSquares} is {(EnoughMovementLeft ? "enough" : "not enough")} to pay for it [{Authority}]";
}

/// <summary>
/// Whether a step may cross a corner (<c>grid-corners</c>): "Diagonal movement can't cross the corner
/// of a wall, a large tree, or another terrain feature that fills its space."
/// </summary>
/// <param name="Allowed">True when the rule does not stop the step.</param>
/// <param name="Adjacency">The step; the rule speaks only of diagonal movement.</param>
/// <param name="Blocking">The feature that fills its space and stops the step, or null when none does.</param>
/// <param name="AtTheCorner">Every feature the caller stated at the corner.</param>
/// <param name="Play">The grid-play statement it was resolved under.</param>
/// <param name="Authority">The rule: <c>grid-corners</c>, "Combat / Playing on a Grid / p. 13".</param>
public sealed record CornerCrossing(
    bool Allowed,
    SquareAdjacency Adjacency,
    TerrainFeature? Blocking,
    ImmutableArray<TerrainFeature> AtTheCorner,
    GridPlayStatement Play,
    SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        Allowed
            ? $"the {Adjacency.ToString().ToLowerInvariant()} step may be made [{Authority}]"
            : $"the diagonal step can't cross the corner of {Blocking?.Name} [{Authority}]";
}
