using RulesKernel.Provenance;
using Srd52Combat.Initiative;

namespace Srd52Combat.Cover;

/// <summary>
/// What the Cover table's "Offered By" column names as the thing that offers cover
/// (<c>cover-degree</c>, "Combat / Cover / p. 15"): another creature, or an object. Which one an
/// obstacle is, is a fact the caller states. There is no default: <c>default</c> is refused.
/// </summary>
public enum Obstacle
{
    /// <summary>Another creature. The table names a creature only in its Half row.</summary>
    Creature = 1,

    /// <summary>An object: a wall, a tree, and the like.</summary>
    Object = 2,
}

/// <summary>
/// A degree of cover, as the Cover table's first column lists them (<c>cover-degree</c>, "Combat /
/// Cover / p. 15"), with the case the table gives no row: no cover at all.
/// </summary>
public enum CoverDegree
{
    /// <summary>No degree of the table is offered.</summary>
    None = 1,

    /// <summary>Half Cover: "Another creature or an object that covers at least half of the target".</summary>
    Half = 2,

    /// <summary>Three-Quarters Cover: "An object that covers at least three-quarters of the target".</summary>
    ThreeQuarters = 3,

    /// <summary>Total Cover: "An object that covers the whole target".</summary>
    Total = 4,
}

/// <summary>
/// One thing between attacker and target, as the caller states it: what it is, and how much of the
/// target it covers. "The fraction of a target an object covers is a parameter" (the entry's note),
/// so the engine is told it and never measures one.
/// </summary>
/// <param name="Kind">A creature or an object.</param>
/// <param name="Id">The obstacle's name or handle.</param>
/// <param name="PercentOfTheTarget">How much of the target it covers, from 0 to 100 per cent.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record CoveringObstacle(Obstacle Kind, string Id, int PercentOfTheTarget, string StatedBy)
{
    /// <summary>The kind, checked to be stated.</summary>
    public Obstacle Kind { get; } = Checks.Defined(Kind, nameof(Kind));

    /// <summary>The obstacle, checked to be named.</summary>
    public string Id { get; } = Checks.Text(Id, nameof(Id));

    /// <summary>The fraction covered, checked to be a percentage.</summary>
    public int PercentOfTheTarget { get; } = PercentOfTheTarget is >= 0 and <= Whole
        ? PercentOfTheTarget
        : throw new ArgumentOutOfRangeException(
            nameof(PercentOfTheTarget), PercentOfTheTarget, "the fraction of the target covered runs from 0 to 100 per cent");

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>Covering the whole target, as the Total row requires.</summary>
    public const int Whole = 100;

    /// <summary>Covering at least three-quarters of the target, as the Three-Quarters row requires.</summary>
    public const int ThreeQuarters = 75;

    /// <summary>Covering at least half of the target, as the Half row requires.</summary>
    public const int Half = 50;

    /// <summary>A creature between attacker and target, covering the fraction stated.</summary>
    /// <param name="id">The creature's name or handle.</param>
    /// <param name="percentOfTheTarget">How much of the target it covers.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static CoveringObstacle ACreature(string id, int percentOfTheTarget, string statedBy) =>
        new(Obstacle.Creature, id, percentOfTheTarget, statedBy);

    /// <summary>An object between attacker and target, covering the fraction stated.</summary>
    /// <param name="id">The object's name or handle.</param>
    /// <param name="percentOfTheTarget">How much of the target it covers.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static CoveringObstacle AnObject(string id, int percentOfTheTarget, string statedBy) =>
        new(Obstacle.Object, id, percentOfTheTarget, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Id} ({(Kind == Obstacle.Creature ? "another creature" : "an object")} covering {PercentOfTheTarget} per cent of the target), as stated by {StatedBy}";
}

/// <summary>
/// The degree of cover one obstacle gives a target: <c>cover-degree</c>, "Combat / Cover / p. 15",
/// the Cover table's "Offered By" column. What each degree is worth is <c>cover-bonuses</c>, and
/// what Total Cover forbids is <c>total-cover</c>; neither is built, and neither is said here.
/// </summary>
/// <param name="Degree">The degree the table gives.</param>
/// <param name="Obstacle">The obstacle it is given by; null when the caller states none.</param>
/// <param name="Because">Why, in the engine's words, naming the row of the table.</param>
/// <param name="Authority">The rule: "Combat / Cover / p. 15".</param>
public sealed record CoverRuling(CoverDegree Degree, CoveringObstacle? Obstacle, string Because, SourceLocator Authority)
{
    /// <summary>The degree in the table's own words: "Half", "Three-Quarters", "Total".</summary>
    public string DegreeAsPrinted => Degree switch
    {
        CoverDegree.Half => "Half Cover",
        CoverDegree.ThreeQuarters => "Three-Quarters Cover",
        CoverDegree.Total => "Total Cover",
        _ => "no cover",
    };

    /// <inheritdoc/>
    public override string ToString() => $"{DegreeAsPrinted}: {Because} [{Authority.Citation}]";
}
