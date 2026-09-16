using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.Fixtures;
using static Srd52Combat.Tests.GridEntryPointTests;

namespace Srd52Combat.Tests;

/// <summary>
/// <c>grid-entering-square</c>, "Combat / Playing on a Grid / p. 13", through
/// <see cref="EntryPoints.GridEnteringSquare"/> only: what a square costs to enter, that a diagonal
/// step costs what an orthogonal one costs, and the occupied square the rule does not price.
/// </summary>
public class GridEnteringSquareEntryPointTests
{
    [Fact]
    public void Entering_an_unoccupied_adjacent_square_costs_one_square_diagonally_as_orthogonally_citing_page_13()
    {
        var orthogonal = Value<SquareEntry>(Enter(SquareStatement.Unoccupied(Gm), SquareAdjacency.Orthogonal));
        var diagonal = Value<SquareEntry>(Enter(SquareStatement.Unoccupied(Gm), SquareAdjacency.Diagonal));

        Assert.Equal(1, orthogonal.Cost);
        Assert.Equal(orthogonal.Cost, diagonal.Cost);
        Assert.True(diagonal.EnoughMovementLeft);
        Assert.Equal(EntryPoints.GridEnteringSquare.Registered.Locator, diagonal.Authority);
        Assert.Equal("Combat / Playing on a Grid / p. 13", diagonal.Authority.Citation);
    }

    [Fact]
    public void A_square_of_Difficult_Terrain_costs_two_squares_to_enter()
    {
        var difficult = Value<SquareEntry>(Enter(SquareStatement.UnoccupiedDifficultTerrain(Gm), SquareAdjacency.Orthogonal));

        Assert.Equal(2, difficult.Cost);
    }

    [Fact]
    public void A_square_held_by_a_creature_that_is_neither_Tiny_nor_your_ally_is_Difficult_Terrain_and_costs_two()
    {
        var ogre = CreatureInSpace.Stranger("an ogre", CreatureSize.Large, Gm);

        var held = Value<SquareEntry>(Enter(SquareStatement.OccupiedBy(ogre, Gm), SquareAdjacency.Diagonal));

        Assert.Equal(2, held.Cost);
    }

    [Fact]
    public void A_square_held_by_a_Tiny_creature_or_an_ally_declines_RequiresInterpretation_citing_the_entry()
    {
        foreach (var occupant in new[]
        {
            CreatureInSpace.Ally("Bram", CreatureSize.Medium, Gm),
            CreatureInSpace.Stranger("a rat", CreatureSize.Tiny, Gm),
        })
        {
            var declined = Declined(Enter(SquareStatement.OccupiedBy(occupant, Gm), SquareAdjacency.Orthogonal));

            Assert.Equal(UnresolvedReason.RequiresInterpretation, declined.Reason);
            Assert.Equal(EntryPoints.GridEnteringSquare.Registered.Locator, declined.Locator);
            Assert.Contains("'grid-entering-square'", declined.Attempted, StringComparison.Ordinal);
            Assert.Contains(occupant.Id, declined.Attempted, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void A_square_that_is_both_held_by_an_ally_and_Difficult_Terrain_costs_two()
    {
        // The Difficult Terrain the caller states prices the square at 2 whoever stands in it; what
        // the rule leaves unpriced is the square that is occupied and not Difficult Terrain.
        var ally = CreatureInSpace.Ally("Bram", CreatureSize.Medium, Gm);

        var entry = Value<SquareEntry>(Enter(new SquareStatement(DifficultTerrain: true, ally, Gm), SquareAdjacency.Orthogonal));

        Assert.Equal(2, entry.Cost);
    }

    [Fact]
    public void Entering_needs_enough_movement_left_to_pay_for_it()
    {
        var difficult = SquareStatement.UnoccupiedDifficultTerrain(Gm);

        var oneLeft = Value<SquareEntry>(Enter(difficult, SquareAdjacency.Orthogonal, movementLeft: 1));
        var twoLeft = Value<SquareEntry>(Enter(difficult, SquareAdjacency.Orthogonal, movementLeft: 2));
        var noneLeft = Value<SquareEntry>(Enter(SquareStatement.Unoccupied(Gm), SquareAdjacency.Orthogonal, movementLeft: 0));

        Assert.False(oneLeft.EnoughMovementLeft);
        Assert.True(twoLeft.EnoughMovementLeft);
        Assert.False(noneLeft.EnoughMovementLeft);
    }

    [Fact]
    public void Off_the_grid_entering_a_square_declines_citing_grid_play()
    {
        var declined = Declined(EntryPoints.GridEnteringSquare.Resolve(new GridEnteringSquareRequest
        {
            Square = SquareStatement.Unoccupied(Gm),
            Adjacency = SquareAdjacency.Orthogonal,
            MovementLeftInSquares = 6,
            Play = Offhand,
        }));

        Assert.Equal(UnresolvedReason.OutsideCurrentScope, declined.Reason);
        Assert.Equal(EntryPoints.GridPlay.Registered.Locator, declined.Locator);
        Assert.Contains("'grid-entering-square'", declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void A_statement_about_the_square_left_out_is_refused_never_inferred()
    {
        var noSquare = Assert.Throws<ArgumentException>(() => EntryPoints.GridEnteringSquare.Resolve(
            new GridEnteringSquareRequest { Adjacency = SquareAdjacency.Orthogonal, MovementLeftInSquares = 6, Play = OnAGrid }));
        Assert.Equal("Square", noSquare.ParamName);

        var noMovementLeft = Assert.Throws<ArgumentException>(() => EntryPoints.GridEnteringSquare.Resolve(
            new GridEnteringSquareRequest { Square = SquareStatement.Unoccupied(Gm), Adjacency = SquareAdjacency.Orthogonal, Play = OnAGrid }));
        Assert.Equal("MovementLeftInSquares", noMovementLeft.ParamName);
    }

    private static Resolution<object> Enter(SquareStatement square, SquareAdjacency adjacency, int movementLeft = 6) =>
        EntryPoints.GridEnteringSquare.Resolve(new GridEnteringSquareRequest
        {
            Square = square,
            Adjacency = adjacency,
            MovementLeftInSquares = movementLeft,
            Play = OnAGrid,
        });
}
