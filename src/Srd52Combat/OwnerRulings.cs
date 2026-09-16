using System.Collections.Immutable;

namespace Srd52Combat;

/// <summary>
/// This engine's additions to the <see cref="OwnerRuling"/> rules-factory generates from the overlay's
/// <c>rulings</c> (<c>Generated/Rulings.g.cs</c>, rules-factory decision 0027): a name for each ruling
/// that says what it rules, and the order every result lists its rulings in. The metadata is the
/// overlay's, and nothing here restates it.
/// </summary>
public static partial class OwnerRulings
{
    /// <summary><c>group-initiative/no-grouping</c>: every creature rolls its own Initiative and the engine never groups (<c>docs/decisions/0007</c>).</summary>
    public static OwnerRuling EveryCreatureRollsItsOwnInitiative => GroupInitiativeNoGrouping;

    /// <summary><c>next-round/agreement-ends-it</c>: both sides agreeing ends the combat, though neither is defeated (<c>docs/decisions/0007</c>).</summary>
    public static OwnerRuling AgreementEndsTheCombat => NextRoundAgreementEndsIt;

    /// <summary><c>moving-through-creatures/two-or-more</c>: "two sizes larger or smaller" means two or more (<c>docs/decisions/0007</c>).</summary>
    public static OwnerRuling TwoSizesMeansTwoOrMore => MovingThroughCreaturesTwoOrMore;

    /// <summary><c>appropriate-anatomy/gm-decides</c>: an appropriate anatomy is the GM's call, stated to the engine (<c>docs/decisions/0007</c>).</summary>
    public static OwnerRuling TheGmDecidesTheAnatomy => AppropriateAnatomyGmDecides;

    /// <summary><c>mount-control-requires-training/training-is-stated</c>: training to accept a rider is a fact the caller supplies (<c>docs/decisions/0007</c>).</summary>
    public static OwnerRuling TrainingIsAStatedFact => MountControlRequiresTrainingTrainingIsStated;

    /// <summary>
    /// <paramref name="rulings"/> without repeats, in <see cref="All"/>'s order, which is the overlay's:
    /// the one order every result lists its rulings in, whatever order they were relied on. A result
    /// derived from results that rely on rulings names their union this way (rules-factory decision
    /// 0027 § 4, as amended on 2026-09-15).
    /// </summary>
    /// <param name="rulings">The rulings the result relied on, in any order and with any repeats.</param>
    /// <returns>Them, once each, in overlay order.</returns>
    public static ImmutableArray<OwnerRuling> InOrder(IEnumerable<OwnerRuling> rulings)
    {
        ArgumentNullException.ThrowIfNull(rulings);
        var relied = rulings.ToHashSet();
        return [.. All.Where(relied.Contains)];
    }

    /// <summary>No ruling was relied on.</summary>
    public static ImmutableArray<OwnerRuling> None { get; } = [];
}
