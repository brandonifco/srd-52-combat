using System.Collections.Immutable;
using RulesKernel.Provenance;
using Srd52Combat.Initiative;

namespace Srd52Combat.Turn;

/// <summary>
/// What <c>group-initiative</c> ("Combat / Initiative / p. 13") answers: "The GM rolls for
/// monsters." The other half — that a group of identical creatures takes a single roll — turns on
/// what makes creatures such a group, which the corpus does not say. Brandon ruled on 2026-09-15
/// that every creature rolls its own Initiative and this engine never groups
/// (<c>group-initiative/no-grouping</c>, <c>docs/decisions/0007</c>), so a stated group is recorded,
/// changes no roll, and the answer names the ruling.
/// </summary>
/// <param name="MonstersTheGmRollsFor">Every participant the caller states is a monster, in the order given.</param>
/// <param name="Groups">The identical-creatures statement this was answered under, whether or not it names a group.</param>
/// <param name="StatedBy">Who stated the participants and their kinds.</param>
/// <param name="Authority">The rule: <c>group-initiative</c>, "Combat / Initiative / p. 13".</param>
/// <param name="Rulings">
/// The owner's rulings this answer relies on: <c>group-initiative/no-grouping</c> where a group was
/// stated, and none where none was, because the corpus's own sentence answers that on its own.
/// </param>
public sealed record GroupInitiativeRolls(
    ImmutableArray<string> MonstersTheGmRollsFor,
    IdenticalCreaturesStatement Groups,
    string StatedBy,
    SourceLocator Authority,
    ImmutableArray<OwnerRuling> Rulings)
{
    /// <summary>The rulings, checked to be present, even when there are none.</summary>
    public ImmutableArray<OwnerRuling> Rulings { get; } =
        Rulings.IsDefault ? throw new ArgumentNullException(nameof(Rulings)) : Rulings;

    /// <summary>
    /// The d20s this rule draws: none. Under the owner's ruling there is no group roll to make, and
    /// every participant's roll is <c>initiative-roll</c>'s.
    /// </summary>
    public int Draws => 0;

    /// <inheritdoc/>
    public override string ToString() =>
        MonstersTheGmRollsFor.IsEmpty
            ? $"no participant is a monster, so this rule gives the GM no roll [{Authority.Citation}]"
            : $"the GM rolls for {string.Join(", ", MonstersTheGmRollsFor)} [{Authority.Citation}]";
}
