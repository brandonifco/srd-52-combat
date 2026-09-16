using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// "Combat / Moving around Other Creatures / p. 14" through its entry points only: whose space you
/// can pass through (<c>moving-through-creatures</c>), whose space is Difficult Terrain
/// (<c>creature-space-difficult-terrain</c>), where a move may end
/// (<c>no-willing-end-in-occupied-space</c>) and what ending a turn in a shared space does
/// (<c>ending-turn-in-occupied-space</c>).
/// </summary>
public class MovingAroundCreaturesEntryPointTests
{
    [Fact]
    public void You_can_pass_through_an_ally_an_Incapacitated_or_Tiny_creature_or_one_exactly_two_sizes_away_citing_page_14()
    {
        foreach (var (yourSize, other, because) in new[]
        {
            (CreatureSize.Medium, CreatureInSpace.Ally("Bram", CreatureSize.Medium, Gm), "ally"),
            (CreatureSize.Medium, CreatureInSpace.Incapacitated("a downed orc", CreatureSize.Medium, Gm), "Incapacitated"),
            (CreatureSize.Medium, CreatureInSpace.Stranger("a rat", CreatureSize.Tiny, Gm), "Tiny"),
            // Two sizes larger, and two sizes smaller: Medium to Huge, and Large to Small.
            (CreatureSize.Medium, CreatureInSpace.Stranger("a giant", CreatureSize.Huge, Gm), "two sizes"),
            (CreatureSize.Large, CreatureInSpace.Stranger("a sprite", CreatureSize.Small, Gm), "two sizes"),
        })
        {
            var ruling = Value<PassageRuling>(Pass(yourSize, other));

            Assert.True(ruling.MayPassThrough);
            Assert.Contains(because, ruling.Because, StringComparison.Ordinal);
            Assert.Equal(EntryPoints.MovingThroughCreatures.Registered.Locator, ruling.Authority);
            Assert.Equal("Combat / Moving around Other Creatures / p. 14", ruling.Authority.Citation);

            // Each of the four is the sentence's own case, and rests on no ruling of the owner's.
            Assert.Empty(ruling.Rulings);
        }
    }

    [Fact]
    public void A_creature_more_than_two_sizes_away_may_be_passed_through_naming_the_owners_ruling()
    {
        foreach (var (yourSize, other) in new[]
        {
            // Three sizes larger, and three sizes smaller.
            (CreatureSize.Medium, CreatureInSpace.Stranger("a dragon", CreatureSize.Gargantuan, Gm)),
            (CreatureSize.Gargantuan, CreatureInSpace.Stranger("a bandit", CreatureSize.Medium, Gm)),
        })
        {
            var ruling = Value<PassageRuling>(Pass(yourSize, other));

            Assert.True(ruling.MayPassThrough);
            Assert.Contains("two or more", ruling.Because, StringComparison.Ordinal);

            // Read literally the sentence names only a creature exactly two sizes away. That it
            // reaches further is Brandon's ruling, and the answer says whose it is.
            var owners = Assert.Single(ruling.Rulings);
            Assert.Equal("moving-through-creatures/two-or-more", owners.Id);
            Assert.Equal("moving-through-creatures", owners.EntryId);
            Assert.Equal("Brandon", owners.RuledBy);
            Assert.Equal(new DateOnly(2026, 9, 16), owners.RuledOn);
            Assert.Equal("docs/decisions/0007-brandons-rulings-on-six-open-questions-are-ruleset-version-seven.md", owners.Record);
            Assert.Contains("exactly two sizes, or two or more", owners.Span, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void A_creature_fewer_than_two_sizes_away_is_the_part_of_the_question_nobody_has_ruled_on()
    {
        foreach (var (yourSize, other) in new[]
        {
            // Fewer than two sizes away and none of the other cases: the sentence does not say that
            // every other creature's space cannot be passed through, and nobody has ruled that it does.
            (CreatureSize.Medium, CreatureInSpace.Stranger("a bandit", CreatureSize.Medium, Gm)),
            (CreatureSize.Medium, CreatureInSpace.Stranger("an ogre", CreatureSize.Large, Gm)),
        })
        {
            var declined = Declined(Pass(yourSize, other));

            Assert.Equal(UnresolvedReason.RequiresInterpretation, declined.Reason);
            Assert.Equal(EntryPoints.MovingThroughCreatures.Registered.Locator, declined.Locator);
            Assert.Contains("'moving-through-creatures'", declined.Attempted, StringComparison.Ordinal);
            Assert.Contains(other.Id, declined.Attempted, StringComparison.Ordinal);
            Assert.Contains("does not say that every other creature's space", declined.Attempted, StringComparison.Ordinal);

            // This is the span the overlay's `declines` names, so it says nothing of the ruling.
            Assert.DoesNotContain("two or more", declined.Attempted, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Another_creatures_space_is_Difficult_Terrain_unless_it_is_Tiny_or_your_ally_citing_page_14()
    {
        var stranger = Value<CreatureSpaceTerrain>(Space(CreatureInSpace.Stranger("a bandit", CreatureSize.Medium, Gm)));
        var ally = Value<CreatureSpaceTerrain>(Space(CreatureInSpace.Ally("Bram", CreatureSize.Medium, Gm)));
        var tiny = Value<CreatureSpaceTerrain>(Space(CreatureInSpace.Stranger("a rat", CreatureSize.Tiny, Gm)));
        var incapacitated = Value<CreatureSpaceTerrain>(Space(CreatureInSpace.Incapacitated("a downed orc", CreatureSize.Medium, Gm)));

        Assert.True(stranger.DifficultTerrainForYou);
        Assert.False(ally.DifficultTerrainForYou);
        Assert.False(tiny.DifficultTerrainForYou);
        // The rule names two exceptions, Tiny and your ally, and no more: an Incapacitated creature's
        // space can be passed through and is still Difficult Terrain.
        Assert.True(incapacitated.DifficultTerrainForYou);
        Assert.Equal(EntryPoints.CreatureSpaceDifficultTerrain.Registered.Locator, stranger.Authority);
        Assert.Equal("Combat / Moving around Other Creatures / p. 14", stranger.Authority.Citation);
    }

    [Fact]
    public void A_move_cant_willingly_end_in_an_occupied_space_and_an_unwilling_one_is_not_forbidden_citing_page_14()
    {
        var bandit = CreatureInSpace.Stranger("a bandit", CreatureSize.Medium, Gm);
        var ally = CreatureInSpace.Ally("Bram", CreatureSize.Medium, Gm);

        var willing = Value<EndOfMoveRuling>(EndMove(bandit, willingly: true));
        var onAnAlly = Value<EndOfMoveRuling>(EndMove(ally, willingly: true));
        var forced = Value<EndOfMoveRuling>(EndMove(bandit, willingly: false));
        var empty = Value<EndOfMoveRuling>(EndMove(null, willingly: true));

        Assert.False(willing.Allowed);
        // The rule says "another creature", with no exception for an ally or a Tiny creature.
        Assert.False(onAnAlly.Allowed);
        Assert.True(forced.Allowed);
        Assert.True(empty.Allowed);
        Assert.Equal(EntryPoints.NoWillingEndInOccupiedSpace.Registered.Locator, willing.Authority);
        Assert.Equal("Combat / Moving around Other Creatures / p. 14", willing.Authority.Citation);
    }

    [Fact]
    public void Ending_a_turn_in_a_space_with_a_creature_of_the_same_size_leaves_you_Prone_and_one_size_larger_does_not_citing_page_14()
    {
        var ogre = CreatureInSpace.Stranger("an ogre", CreatureSize.Large, Gm);
        var bandit = CreatureInSpace.Stranger("a bandit", CreatureSize.Medium, Gm);

        var sameSize = Value<EndOfTurnRuling>(EndTurn(CreatureSize.Medium, bandit));
        var oneLarger = Value<EndOfTurnRuling>(EndTurn(CreatureSize.Large, bandit));
        var smaller = Value<EndOfTurnRuling>(EndTurn(CreatureSize.Medium, ogre));
        var tiny = Value<EndOfTurnRuling>(EndTurn(CreatureSize.Tiny, bandit));

        Assert.True(sameSize.Prone);
        Assert.False(oneLarger.Prone);
        Assert.True(smaller.Prone);
        Assert.False(tiny.Prone);
        Assert.Equal(EntryPoints.EndingTurnInOccupiedSpace.Registered.Locator, sameSize.Authority);
        Assert.Equal("Combat / Moving around Other Creatures / p. 14", sameSize.Authority.Citation);
    }

    [Fact]
    public void A_creature_that_shared_the_space_during_the_turn_but_not_at_its_end_is_not_Prone()
    {
        // The prohibition is on ending a move in the space; the Prone consequence is on ending a turn
        // there. A creature pushed into a space mid-turn that leaves before the turn ends is not Prone.
        var left = Value<EndOfTurnRuling>(EntryPoints.EndingTurnInOccupiedSpace.Resolve(new EndingTurnInOccupiedSpaceRequest
        {
            YourSize = CreatureSize.Medium,
            Turn = EndOfTurnStatement.SharedWithNobody(Gm),
        }));

        Assert.False(left.Prone);
        Assert.Null(left.Other);
    }

    [Fact]
    public void A_statement_about_another_creature_left_out_is_refused_never_inferred()
    {
        var noSize = Assert.Throws<ArgumentException>(() => EntryPoints.MovingThroughCreatures.Resolve(
            new MovingThroughCreaturesRequest { Other = CreatureInSpace.Stranger("a bandit", CreatureSize.Medium, Gm) }));
        Assert.Equal("YourSize", noSize.ParamName);

        var noOther = Assert.Throws<ArgumentException>(() => EntryPoints.MovingThroughCreatures.Resolve(
            new MovingThroughCreaturesRequest { YourSize = CreatureSize.Medium }));
        Assert.Equal("Other", noOther.ParamName);

        var noSpaceHolder = Assert.Throws<ArgumentException>(() =>
            EntryPoints.CreatureSpaceDifficultTerrain.Resolve(new CreatureSpaceDifficultTerrainRequest()));
        Assert.Equal("Other", noSpaceHolder.ParamName);

        var noWillingness = Assert.Throws<ArgumentException>(() => EntryPoints.NoWillingEndInOccupiedSpace.Resolve(
            new NoWillingEndInOccupiedSpaceRequest { Occupant = CreatureInSpace.Stranger("a bandit", CreatureSize.Medium, Gm) }));
        Assert.Equal("Willingly", noWillingness.ParamName);

        var noTurn = Assert.Throws<ArgumentException>(() => EntryPoints.EndingTurnInOccupiedSpace.Resolve(
            new EndingTurnInOccupiedSpaceRequest { YourSize = CreatureSize.Medium }));
        Assert.Equal("Turn", noTurn.ParamName);

        // A size is never inferred, and a turn that ended in a space with a creature names it.
        Assert.Throws<ArgumentException>(() => CreatureInSpace.Stranger("a bandit", default, Gm));
        Assert.Throws<ArgumentException>(() => EndOfTurnStatement.SharedWith(null!, Gm));
    }

    private static Resolution<object> Pass(CreatureSize yourSize, CreatureInSpace other) =>
        EntryPoints.MovingThroughCreatures.Resolve(new MovingThroughCreaturesRequest { YourSize = yourSize, Other = other });

    private static Resolution<object> Space(CreatureInSpace other) =>
        EntryPoints.CreatureSpaceDifficultTerrain.Resolve(new CreatureSpaceDifficultTerrainRequest { Other = other });

    private static Resolution<object> EndMove(CreatureInSpace? occupant, bool willingly) =>
        EntryPoints.NoWillingEndInOccupiedSpace.Resolve(new NoWillingEndInOccupiedSpaceRequest { Occupant = occupant, Willingly = willingly });

    private static Resolution<object> EndTurn(CreatureSize yourSize, CreatureInSpace other) =>
        EntryPoints.EndingTurnInOccupiedSpace.Resolve(new EndingTurnInOccupiedSpaceRequest
        {
            YourSize = yourSize,
            Turn = EndOfTurnStatement.SharedWith(other, Gm),
        });
}
