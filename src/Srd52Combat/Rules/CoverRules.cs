using RulesKernel.Resolution;
using Srd52Combat.Cover;

namespace Srd52Combat.Rules;

/// <summary>
/// Cover, "Combat / Cover / p. 15": the degree an obstacle offers (<c>cover-degree</c>), which is
/// the Cover table's "Offered By" column and nothing else.
/// </summary>
/// <remarks>
/// <para>
/// What each degree is worth (<c>cover-bonuses</c>), whether cover counts against this attack at all
/// (<c>cover-origin</c>), what Total Cover forbids (<c>total-cover</c>) and which of several degrees
/// applies (<c>cover-no-stacking</c>) are four other entries, none of them built. This rule answers
/// the degree one stated obstacle gives, and declines
/// <see cref="UnresolvedReason.UnsupportedRule"/> citing <c>cover-no-stacking</c> when the caller
/// states more than one, because choosing among them is that entry's rule and not this one's.
/// </para>
/// <para>
/// The entry's question is unresolved: does "that covers at least half of the target" qualify
/// "Another creature" as well as "an object"? It bites in exactly one case, a creature covering less
/// than half of the target: on one reading it gives Half Cover, on the other it gives none. That
/// case declines <see cref="UnresolvedReason.RequiresInterpretation"/> citing the entry. Every other
/// case is the same under both readings, and is answered: a creature covering at least half gives
/// Half Cover either way, and a creature never gives more, because the Three-Quarters and Total rows
/// name only an object (the entry's note).
/// </para>
/// </remarks>
public static class CoverRules
{
    /// <summary>
    /// <c>cover-degree</c>: the Cover table's "Offered By" column. "Half … Another creature or an
    /// object that covers at least half of the target. Three-Quarters … An object that covers at
    /// least three-quarters of the target. Total … An object that covers the whole target."
    /// </summary>
    /// <param name="obstacles">What the caller states lies between attacker and target; empty when nothing does.</param>
    /// <returns>The degree of cover, or a decline.</returns>
    /// <exception cref="ArgumentException"><paramref name="obstacles"/> contains a null.</exception>
    public static Resolution<CoverRuling> Degree(IReadOnlyList<CoveringObstacle> obstacles)
    {
        ArgumentNullException.ThrowIfNull(obstacles);
        if (obstacles.Any(o => o is null))
        {
            throw new ArgumentException("no obstacle stated is null", nameof(obstacles));
        }

        string attempting = Declines.Attempting(MapEntries.CoverDegree);
        if (obstacles.Count == 0)
        {
            return Resolution<CoverRuling>.FromValue(new CoverRuling(
                CoverDegree.None,
                null,
                "nothing is stated between the attacker and the target, and the table gives a degree only for an obstacle",
                MapEntries.CoverDegree.Locator));
        }

        if (obstacles.Count > 1)
        {
            return Declines.Of<CoverRuling>(
                UnresolvedReason.UnsupportedRule,
                $"{attempting} behind {obstacles.Count} stated sources of cover ({string.Join("; ", obstacles)}): "
                + $"which degree applies to a target behind more than one is '{MapEntries.CoverNoStacking.Id}', which is not built",
                MapEntries.CoverNoStacking);
        }

        var obstacle = obstacles[0];
        int covered = obstacle.PercentOfTheTarget;
        if (obstacle.Kind == Obstacle.Creature)
        {
            return covered >= CoveringObstacle.Half
                ? Resolution<CoverRuling>.FromValue(new CoverRuling(
                    CoverDegree.Half,
                    obstacle,
                    $"the Half row is offered by \"Another creature or an object that covers at least half of the target\", and {obstacle}; "
                    + "the Three-Quarters and Total rows name only an object, so a creature gives no more than Half Cover",
                    MapEntries.CoverDegree.Locator))
                : Declines.Of<CoverRuling>(
                    UnresolvedReason.RequiresInterpretation,
                    $"{attempting} for {obstacle}: whether \"that covers at least half of the target\" qualifies \"Another creature\" "
                    + "as well as \"an object\" decides this case and nothing else, and the corpus does not choose",
                    MapEntries.CoverDegree);
        }

        (CoverDegree degree, string? row) = covered >= CoveringObstacle.Whole
            ? (CoverDegree.Total, "An object that covers the whole target")
            : covered >= CoveringObstacle.ThreeQuarters
                ? (CoverDegree.ThreeQuarters, "An object that covers at least three-quarters of the target")
                : covered >= CoveringObstacle.Half
                    ? (CoverDegree.Half, "Another creature or an object that covers at least half of the target")
                    : (CoverDegree.None, null);

        return Resolution<CoverRuling>.FromValue(new CoverRuling(
            degree,
            obstacle,
            degree == CoverDegree.None
                ? $"{obstacle}, and the least degree the table offers needs at least half of the target covered"
                : $"the row is offered by \"{row}\", and {obstacle}",
            MapEntries.CoverDegree.Locator));
    }
}
