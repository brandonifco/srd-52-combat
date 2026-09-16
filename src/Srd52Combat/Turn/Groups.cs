using System.Collections.Immutable;
using RulesKernel.Provenance;
using Srd52Combat.Initiative;

namespace Srd52Combat.Turn;

/// <summary>
/// What <c>group-initiative</c> ("Combat / Initiative / p. 13") answers where its unresolved
/// question does not arise: "The GM rolls for monsters." The other half — that a group of identical
/// creatures takes a single roll — turns on what makes creatures such a group, which the corpus
/// does not say, so the engine declines it and draws nothing.
/// </summary>
/// <param name="MonstersTheGmRollsFor">Every participant the caller states is a monster, in the order given.</param>
/// <param name="Groups">The identical-creatures statement this was answered under; it names no group, or the rule declines.</param>
/// <param name="StatedBy">Who stated the participants and their kinds.</param>
/// <param name="Authority">The rule: <c>group-initiative</c>, "Combat / Initiative / p. 13".</param>
public sealed record GroupInitiativeRolls(
    ImmutableArray<string> MonstersTheGmRollsFor,
    IdenticalCreaturesStatement Groups,
    string StatedBy,
    SourceLocator Authority)
{
    /// <summary>
    /// The d20s this rule draws: none. A single roll for a group is the half that cannot be
    /// resolved, and every other participant's roll is <c>initiative-roll</c>'s.
    /// </summary>
    public int Draws => 0;

    /// <inheritdoc/>
    public override string ToString() =>
        MonstersTheGmRollsFor.IsEmpty
            ? $"no participant is a monster, so this rule gives the GM no roll [{Authority.Citation}]"
            : $"the GM rolls for {string.Join(", ", MonstersTheGmRollsFor)} [{Authority.Citation}]";
}
