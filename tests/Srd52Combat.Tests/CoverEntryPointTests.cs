using RulesKernel.Resolution;
using Srd52Combat.Cover;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// "Combat / Cover / p. 15", through the entry points only: the degree of cover an obstacle gives
/// (<c>cover-degree</c>), which is the Cover table's "Offered By" column.
/// </summary>
public class CoverEntryPointTests
{
    [Fact]
    public void An_object_covering_half_three_quarters_or_the_whole_target_gives_that_degree_citing_page_15()
    {
        (int Percent, CoverDegree Degree)[] cases =
        [
            (50, CoverDegree.Half),
            (74, CoverDegree.Half),
            (75, CoverDegree.ThreeQuarters),
            (99, CoverDegree.ThreeQuarters),
            (100, CoverDegree.Total),
        ];

        foreach (var (percent, degree) in cases)
        {
            var ruling = Value<CoverRuling>(EntryPoints.CoverDegree.Resolve(new CoverDegreeRequest
            {
                Obstacles = [CoveringObstacle.AnObject("the tree trunk", percent, Gm)],
            }));

            Assert.Equal(degree, ruling.Degree);
            Assert.Equal("Combat / Cover / p. 15", ruling.Authority.Citation);
            Assert.Equal(EntryPoints.CoverDegree.Registered.Locator, ruling.Authority);
        }
    }

    [Fact]
    public void An_object_covering_less_than_half_of_the_target_gives_no_degree_of_cover()
    {
        var ruling = Value<CoverRuling>(EntryPoints.CoverDegree.Resolve(new CoverDegreeRequest
        {
            Obstacles = [CoveringObstacle.AnObject("the low fence", 49, Gm)],
        }));

        Assert.Equal(CoverDegree.None, ruling.Degree);

        var nothing = Value<CoverRuling>(EntryPoints.CoverDegree.Resolve(new CoverDegreeRequest { Obstacles = [] }));

        Assert.Equal(CoverDegree.None, nothing.Degree);
        Assert.Null(nothing.Obstacle);
    }

    [Fact]
    public void A_creature_covering_at_least_half_gives_Half_Cover_and_never_more()
    {
        foreach (int percent in new[] { 50, 80, 100 })
        {
            var ruling = Value<CoverRuling>(EntryPoints.CoverDegree.Resolve(new CoverDegreeRequest
            {
                Obstacles = [CoveringObstacle.ACreature("the ogre", percent, Gm)],
            }));

            // The Three-Quarters and Total rows name only an object, so a creature gives no more than Half.
            Assert.Equal(CoverDegree.Half, ruling.Degree);
        }
    }

    [Fact]
    public void A_creature_covering_less_than_half_declines_RequiresInterpretation_citing_cover_degree()
    {
        var declined = Declined(EntryPoints.CoverDegree.Resolve(new CoverDegreeRequest
        {
            Obstacles = [CoveringObstacle.ACreature("the goblin", 30, Gm)],
        }));

        Assert.Equal(UnresolvedReason.RequiresInterpretation, declined.Reason);
        Assert.Equal(EntryPoints.CoverDegree.Registered.Locator, declined.Locator);
        Assert.Contains("'cover-degree'", declined.Attempted, StringComparison.Ordinal);
        Assert.Contains("Another creature", declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void More_than_one_source_of_cover_declines_citing_cover_no_stacking()
    {
        var declined = Declined(EntryPoints.CoverDegree.Resolve(new CoverDegreeRequest
        {
            Obstacles =
            [
                CoveringObstacle.ACreature("the ogre", 60, Gm),
                CoveringObstacle.AnObject("the tree trunk", 80, Gm),
            ],
        }));

        Assert.Equal(UnresolvedReason.UnsupportedRule, declined.Reason);
        Assert.Equal(EntryPoints.CoverNoStacking.Registered.Locator, declined.Locator);
        Assert.Contains("cover-no-stacking", declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void The_obstacles_between_attacker_and_target_are_refused_when_left_out_never_inferred()
    {
        Assert.Throws<ArgumentException>(() => EntryPoints.CoverDegree.Resolve(new CoverDegreeRequest()));
        Assert.Throws<ArgumentOutOfRangeException>(() => CoveringObstacle.AnObject("the wall", 101, Gm));
    }
}
