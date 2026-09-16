using System.Collections.Immutable;
using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Turn;

namespace Srd52Combat.Rules;

/// <summary>
/// The GM's rolls for monsters, "Combat / Initiative / p. 13" (<c>group-initiative</c>): "The GM
/// rolls for monsters. For a group of identical creatures, the GM makes a single roll, so each
/// member of the group has the same Initiative."
/// </summary>
public static class GroupInitiativeRules
{
    /// <summary>
    /// Who the GM rolls for, and how many rolls a group takes.
    /// </summary>
    /// <remarks>
    /// The first sentence is answered: the GM rolls for every participant the caller states is a
    /// monster. The second cannot be. What makes creatures a "group of identical creatures" is not
    /// stated anywhere in the corpus — whether identical means the same stat block, whether every
    /// identical monster forms one group or the GM may divide them, whether a lone monster is a
    /// group of one — and that is what fixes how many d20s are thrown. It is a gap, not a choice the
    /// corpus delegates, so a stated group declines
    /// <see cref="UnresolvedReason.RequiresInterpretation"/> citing <c>group-initiative</c>, and
    /// nothing is drawn: a seeded combat that drew a different number of d20s than the table would
    /// changes every later draw of its replay.
    /// </remarks>
    /// <param name="participants">Every participant, as the caller states them.</param>
    /// <param name="statedBy">Who stated the participants and their kinds.</param>
    /// <param name="identicalCreatures">Which participants form groups of identical creatures. Never defaulted.</param>
    /// <returns>Who the GM rolls for, or the decline.</returns>
    /// <exception cref="ArgumentException">There is no participant, one is null, two share an id, or a group names someone who is not a participant.</exception>
    public static Resolution<GroupInitiativeRolls> Roll(
        IReadOnlyList<Combatant> participants,
        string statedBy,
        IdenticalCreaturesStatement identicalCreatures)
    {
        ArgumentNullException.ThrowIfNull(participants);
        ArgumentException.ThrowIfNullOrWhiteSpace(statedBy);
        ArgumentNullException.ThrowIfNull(identicalCreatures);
        if (participants.Count == 0 || participants.Any(p => p is null))
        {
            throw new ArgumentException("combat has at least one participant, and none is null", nameof(participants));
        }

        var ids = participants.Select(p => p.Id).ToImmutableArray();
        if (ids.Distinct(StringComparer.Ordinal).Count() != ids.Length)
        {
            throw new ArgumentException("every participant has its own id", nameof(participants));
        }

        var unknown = identicalCreatures.Groups.SelectMany(g => g).Where(id => !ids.Contains(id, StringComparer.Ordinal)).ToArray();
        if (unknown.Length > 0)
        {
            throw new ArgumentException(
                $"the identical-creatures statement names {string.Join(", ", unknown)}, who are not participants",
                nameof(identicalCreatures));
        }

        if (!identicalCreatures.Groups.IsEmpty)
        {
            return Resolution<GroupInitiativeRolls>.FromUnresolved(new UnresolvedResult(
                UnresolvedReason.RequiresInterpretation,
                $"resolve the map entry '{MapEntries.GroupInitiative.Id}' [{MapEntries.GroupInitiative.Locator.Citation}] "
                + $"while {identicalCreatures}: the corpus does not say what makes creatures a group of identical creatures, "
                + "so how many rolls are made is not fixed and nothing is drawn",
                MapEntries.GroupInitiative.Locator));
        }

        var monsters = participants.Where(p => p.Kind == CombatantKind.Monster).Select(p => p.Id).ToImmutableArray();
        return Resolution<GroupInitiativeRolls>.FromValue(
            new GroupInitiativeRolls(monsters, identicalCreatures, statedBy, MapEntries.GroupInitiative.Locator));
    }
}
