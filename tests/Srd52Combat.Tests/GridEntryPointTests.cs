using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// The grid variant, "Combat / Playing on a Grid / p. 13", through its entry points only:
/// <c>grid-square-size</c>, <c>grid-speed-in-squares</c> and <c>grid-corners</c>. Every test states
/// whether the table plays on a grid (<c>grid-play</c>, each entry's <c>enabledBy</c>), and each rule
/// is shown not to apply where it does not.
/// </summary>
public class GridEntryPointTests
{
    internal static readonly GridPlayStatement OnAGrid = GridPlayStatement.OnAGrid(Gm);

    internal static readonly GridPlayStatement Offhand = GridPlayStatement.NotOnAGrid(Gm);

    [Fact]
    public void Each_square_represents_five_feet_on_a_grid_citing_page_13()
    {
        var square = Value<GridSquare>(EntryPoints.GridSquareSize.Resolve(new GridSquareSizeRequest { Play = OnAGrid }));

        Assert.Equal(5, square.Feet);
        Assert.Same(OnAGrid, square.Play);
        Assert.Equal("Combat / Playing on a Grid / p. 13", square.Authority.Citation);
        Assert.Equal(EntryPoints.GridSquareSize.Registered.Locator, square.Authority);
    }

    [Fact]
    public void A_Speed_of_thirty_feet_is_six_squares_and_one_of_twelve_feet_is_two_rounded_down_citing_page_13()
    {
        var thirty = Value<SpeedInSquares>(Speed(30));
        var twelve = Value<SpeedInSquares>(Speed(12));

        Assert.Equal(6, thirty.Squares);
        Assert.False(thirty.RoundedDown);
        Assert.Equal(2, twelve.Squares);
        Assert.True(twelve.RoundedDown);
        Assert.Equal(5, thirty.Square.Feet);
        Assert.Equal(0, Value<SpeedInSquares>(Speed(0)).Squares);
        Assert.Equal(EntryPoints.GridSpeedInSquares.Registered.Locator, thirty.Authority);
        Assert.Equal("Combat / Playing on a Grid / p. 13", thirty.Authority.Citation);
    }

    [Fact]
    public void Diagonal_movement_cant_cross_the_corner_of_a_feature_that_fills_its_space_citing_page_13()
    {
        var wall = TerrainFeature.Filling("a wall", Gm);

        var stopped = Value<CornerCrossing>(Corner(SquareAdjacency.Diagonal, wall));

        Assert.False(stopped.Allowed);
        Assert.Same(wall, stopped.Blocking);
        Assert.Equal(EntryPoints.GridCorners.Registered.Locator, stopped.Authority);
        Assert.Equal("Combat / Playing on a Grid / p. 13", stopped.Authority.Citation);
    }

    [Fact]
    public void A_feature_that_does_not_fill_its_space_and_an_orthogonal_step_are_not_stopped()
    {
        var stool = TerrainFeature.NotFilling("a stool", Gm);
        var wall = TerrainFeature.Filling("a wall", Gm);

        var pastTheStool = Value<CornerCrossing>(Corner(SquareAdjacency.Diagonal, stool));
        var orthogonal = Value<CornerCrossing>(Corner(SquareAdjacency.Orthogonal, wall));
        var nothingAtAll = Value<CornerCrossing>(Corner(SquareAdjacency.Diagonal));

        Assert.True(pastTheStool.Allowed);
        Assert.Null(pastTheStool.Blocking);
        // The rule speaks of diagonal movement: an orthogonal step past the same wall is not stopped.
        Assert.True(orthogonal.Allowed);
        Assert.Null(orthogonal.Blocking);
        Assert.True(nothingAtAll.Allowed);
    }

    [Fact]
    public void Off_the_grid_every_grid_rule_declines_citing_grid_play()
    {
        foreach (var (entryId, resolution) in new (string, Resolution<object>)[]
        {
            ("grid-square-size", EntryPoints.GridSquareSize.Resolve(new GridSquareSizeRequest { Play = Offhand })),
            ("grid-speed-in-squares", EntryPoints.GridSpeedInSquares.Resolve(new GridSpeedInSquaresRequest { SpeedInFeet = 30, Play = Offhand })),
            ("grid-corners", EntryPoints.GridCorners.Resolve(new GridCornersRequest
            {
                Adjacency = SquareAdjacency.Diagonal,
                AtTheCorner = [TerrainFeature.Filling("a wall", Gm)],
                Play = Offhand,
            })),
        })
        {
            var declined = Declined(resolution);
            Assert.Equal(UnresolvedReason.OutsideCurrentScope, declined.Reason);
            Assert.Equal(EntryPoints.GridPlay.Registered.Locator, declined.Locator);
            Assert.Contains($"'{entryId}'", declined.Attempted, StringComparison.Ordinal);
            Assert.Contains(Offhand.ToString(), declined.Attempted, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void A_grid_statement_left_out_is_refused_never_inferred()
    {
        var noPlay = Assert.Throws<ArgumentException>(() =>
            EntryPoints.GridSquareSize.Resolve(new GridSquareSizeRequest()));
        Assert.Equal("Play", noPlay.ParamName);

        var noSpeed = Assert.Throws<ArgumentException>(() =>
            EntryPoints.GridSpeedInSquares.Resolve(new GridSpeedInSquaresRequest { Play = OnAGrid }));
        Assert.Equal("SpeedInFeet", noSpeed.ParamName);

        var noAdjacency = Assert.Throws<ArgumentException>(() =>
            EntryPoints.GridCorners.Resolve(new GridCornersRequest { AtTheCorner = [], Play = OnAGrid }));
        Assert.Equal("Adjacency", noAdjacency.ParamName);

        var noFeatures = Assert.Throws<ArgumentException>(() =>
            EntryPoints.GridCorners.Resolve(new GridCornersRequest { Adjacency = SquareAdjacency.Diagonal, Play = OnAGrid }));
        Assert.Equal("AtTheCorner", noFeatures.ParamName);
    }

    private static Resolution<object> Speed(int feet) =>
        EntryPoints.GridSpeedInSquares.Resolve(new GridSpeedInSquaresRequest { SpeedInFeet = feet, Play = OnAGrid });

    private static Resolution<object> Corner(SquareAdjacency adjacency, params TerrainFeature[] atTheCorner) =>
        EntryPoints.GridCorners.Resolve(new GridCornersRequest
        {
            Adjacency = adjacency,
            AtTheCorner = atTheCorner,
            Play = OnAGrid,
        });
}
