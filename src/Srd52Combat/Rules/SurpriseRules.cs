using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Turn;

namespace Srd52Combat.Rules;

/// <summary>
/// Surprise, "Combat / Initiative / p. 13": what follows for a combatant the caller states is
/// surprised (<c>surprise-disadvantage</c>).
/// </summary>
public static class SurpriseRules
{
    /// <summary>
    /// "If a combatant is surprised by combat starting, that combatant has Disadvantage on their
    /// Initiative roll." Given the fact, the consequence, and nothing else: this corpus's only
    /// consequence of surprise is that Disadvantage, and how Disadvantage resolves is
    /// <c>advantage-disadvantage</c> (p. 7), outside the slice, which is why the roll itself still
    /// declines there.
    /// </summary>
    /// <remarks>
    /// Where the GM uses Initiative scores there is no roll for this rule to reach, and the glossary
    /// that turns Disadvantage into 5 off the score is outside the slice, so the rule declines
    /// <see cref="UnresolvedReason.OutsideCurrentScope"/> citing <c>initiative-score-option</c>
    /// (p. 184), the gate <c>initiative-roll</c> answers to as well.
    /// </remarks>
    /// <param name="statement">Whether the combatant is surprised, as stated. Never defaulted.</param>
    /// <param name="scoreOption">Whether the GM uses Initiative scores instead of rolling. Never defaulted.</param>
    /// <returns>What surprise gives this combatant's Initiative roll, or the decline.</returns>
    public static Resolution<SurpriseEffect> OnInitiative(SurprisedStatement statement, InitiativeScoreOptionStatement scoreOption)
    {
        ArgumentNullException.ThrowIfNull(statement);
        ArgumentNullException.ThrowIfNull(scoreOption);
        if (scoreOption.InUse)
        {
            return Resolution<SurpriseEffect>.FromUnresolved(new UnresolvedResult(
                UnresolvedReason.OutsideCurrentScope,
                $"resolve the map entry '{MapEntries.SurpriseDisadvantage.Id}' [{MapEntries.SurpriseDisadvantage.Locator.Citation}] "
                + $"for {statement} while {scoreOption}: there is no Initiative roll to have Disadvantage on, and what Disadvantage does to an Initiative score is "
                + $"'{MapEntries.InitiativeScoreOption.Id}' [{MapEntries.InitiativeScoreOption.Locator.Citation}]",
                MapEntries.InitiativeScoreOption.Locator));
        }

        return Resolution<SurpriseEffect>.FromValue(
            new SurpriseEffect(statement, statement.IsSurprised, MapEntries.SurpriseDisadvantage.Locator));
    }
}
