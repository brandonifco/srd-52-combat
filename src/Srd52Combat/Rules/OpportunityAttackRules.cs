using RulesKernel.Resolution;
using Srd52Combat.Attacks;

namespace Srd52Combat.Rules;

/// <summary>
/// Opportunity Attacks, "Combat / Opportunity Attacks / p. 15": making one
/// (<c>opportunity-attack</c>) and avoiding one (<c>opportunity-attack-avoidance</c>).
/// </summary>
/// <remarks>
/// The trigger the slice states is a creature that the attacker can see leaving the attacker's
/// reach. Three things suspend it, and each is outside the slice, so each declines
/// <see cref="UnresolvedReason.OutsideCurrentScope"/> citing its entry (rules-factory decision
/// 0021): the Incapacitated condition, which allows no Reaction; <c>reactions</c>, while the
/// attacker's Reaction is spent; and the Rules Glossary's Opportunity Attacks entry, which alone
/// says how a creature leaves reach, for a creature that leaves it by none of the means the
/// glossary lists. Avoidance is not one of those: it is a rule of this slice, and answers.
/// </remarks>
public static class OpportunityAttackRules
{
    /// <summary>
    /// <c>opportunity-attack-avoidance</c>: "You can avoid provoking an Opportunity Attack by
    /// taking the Disengage action. You also don't provoke an Opportunity Attack when you Teleport
    /// or when you are moved without using your movement, action, Bonus Action, or Reaction."
    /// </summary>
    /// <param name="leaving">How the creature left reach, and whether it took the Disengage action. Never defaulted.</param>
    /// <returns>Whether the movement provokes, as far as this rule says.</returns>
    public static Resolution<ProvocationVerdict> Avoidance(LeavingReach leaving)
    {
        ArgumentNullException.ThrowIfNull(leaving);
        var locator = MapEntries.OpportunityAttackAvoidance.Locator;
        if (leaving.DisengageTaken)
        {
            return Resolution<ProvocationVerdict>.FromValue(new ProvocationVerdict(
                Provokes: false, $"the creature took the Disengage action; {leaving}", locator));
        }

        return leaving.Means switch
        {
            DepartureMeans.Teleport => Resolution<ProvocationVerdict>.FromValue(new ProvocationVerdict(
                Provokes: false, $"the creature Teleports; {leaving}", locator)),
            DepartureMeans.MovedWithoutItsOwn => Resolution<ProvocationVerdict>.FromValue(new ProvocationVerdict(
                Provokes: false,
                $"the creature is moved without using its movement, action, Bonus Action, or Reaction; {leaving}",
                locator)),
            _ => Resolution<ProvocationVerdict>.FromValue(new ProvocationVerdict(
                Provokes: true,
                $"the creature neither took the Disengage action, nor Teleported, nor was moved without using its own movement, action, Bonus Action, or Reaction; {leaving}",
                locator)),
        };
    }

    /// <summary>
    /// <c>opportunity-attack</c>: "You can make an Opportunity Attack when a creature that you can
    /// see leaves your reach. To make the attack, take a Reaction to make one melee attack with a
    /// weapon or an Unarmed Strike against that creature. The attack occurs right before it leaves
    /// your reach."
    /// </summary>
    /// <param name="sight">Whether the attacker can see the creature. Never defaulted.</param>
    /// <param name="leaving">How the creature leaves reach. Never defaulted.</param>
    /// <param name="reaction">Whether the attacker still has its Reaction. Never defaulted.</param>
    /// <param name="incapacitated">Whether the attacker has the Incapacitated condition. Never defaulted.</param>
    /// <returns>The offer, or the decline.</returns>
    public static Resolution<OpportunityAttackOffer> Make(
        TargetVisibilityStatement sight,
        LeavingReach leaving,
        ReactionAvailability reaction,
        IncapacitatedStatement incapacitated)
    {
        ArgumentNullException.ThrowIfNull(sight);
        ArgumentNullException.ThrowIfNull(leaving);
        ArgumentNullException.ThrowIfNull(reaction);
        ArgumentNullException.ThrowIfNull(incapacitated);

        string attempting = AttackRules.Attempting(MapEntries.OpportunityAttack);
        if (incapacitated.Incapacitated)
        {
            return AttackRules.Decline<OpportunityAttackOffer>(
                UnresolvedReason.OutsideCurrentScope,
                $"{attempting} while {incapacitated}: the attack is made with a Reaction, which an Incapacitated creature cannot take",
                MapEntries.IncapacitatedCondition);
        }

        if (!reaction.Available)
        {
            return AttackRules.Decline<OpportunityAttackOffer>(
                UnresolvedReason.OutsideCurrentScope,
                $"{attempting} while {reaction}: how many Reactions a creature has and when they come back is '{MapEntries.Reactions.Id}'",
                MapEntries.Reactions);
        }

        var avoided = Avoidance(leaving);
        var verdict = avoided.Match<ProvocationVerdict?>(v => v, _ => null);
        if (verdict is { Provokes: false })
        {
            return Resolution<OpportunityAttackOffer>.FromValue(
                OpportunityAttackOffer.None(verdict.Because, verdict.Authority));
        }

        if (sight.CannotSee)
        {
            return Resolution<OpportunityAttackOffer>.FromValue(OpportunityAttackOffer.None(
                $"an Opportunity Attack is made when a creature that you can see leaves your reach, and {sight}",
                MapEntries.OpportunityAttack.Locator));
        }

        if (leaving.Means == DepartureMeans.ByNoneOfThose)
        {
            return AttackRules.Decline<OpportunityAttackOffer>(
                UnresolvedReason.OutsideCurrentScope,
                $"{attempting} where {leaving}: how a creature leaves your reach is the Rules Glossary's "
                + $"('{MapEntries.OpportunityAttacksGlossary.Id}'), which lists its action, its Bonus Action, its Reaction, or one of its speeds",
                MapEntries.OpportunityAttacksGlossary);
        }

        return Resolution<OpportunityAttackOffer>.FromValue(new OpportunityAttackOffer(
            CanBeMade: true,
            UsesReaction: true,
            Attacks: 1,
            [MeleeAttackOption.Weapon, MeleeAttackOption.UnarmedStrike],
            OpportunityAttackOffer.RightBefore,
            $"{sight}, and {leaving}; {reaction}",
            MapEntries.OpportunityAttack.Locator));
    }
}
