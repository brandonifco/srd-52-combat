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
    /// <remarks>
    /// The slice's sentence states no limit on what the Disengage action protects. The Rules
    /// Glossary's <c>disengage-action</c> (p. 181, <c>scope: out</c>, which this entry names in
    /// <c>dependsOn</c>) limits it to "your movement" and to "the rest of the current turn", and the
    /// map records the difference in the entry's <c>note</c> without asking a question
    /// (MAP-FINDINGS finding 11). **Brandon decided on 2026-09-16 that the engine follows the
    /// glossary** (<see cref="OwnerDecisions.DisengageCoversYourOwnMovementThisTurn"/>,
    /// <c>docs/decisions/0007</c>). That is this engine's decision and not an owner's ruling of
    /// rules-factory 0027: the entry is <c>clarity: clear</c>, so it has no open question to hold a
    /// ruling. A verdict the Disengage action decided names the decision in
    /// <see cref="ProvocationVerdict.Decisions"/>; the other limbs are the slice's own and name
    /// nothing.
    /// </remarks>
    /// <param name="leaving">How the creature left reach, and whether it took the Disengage action. Never defaulted.</param>
    /// <returns>Whether the movement provokes, as far as this rule says.</returns>
    public static Resolution<ProvocationVerdict> Avoidance(LeavingReach leaving)
    {
        ArgumentNullException.ThrowIfNull(leaving);
        var locator = MapEntries.OpportunityAttackAvoidance.Locator;
        var followsTheGlossary = OwnerDecisions.DisengageCoversYourOwnMovementThisTurn;
        if (leaving.ProtectedByDisengage)
        {
            return Resolution<ProvocationVerdict>.FromValue(new ProvocationVerdict(
                Provokes: false,
                $"the creature took the Disengage action, and this is its own movement on that turn; {leaving}",
                locator,
                [followsTheGlossary]));
        }

        return leaving.Means switch
        {
            DepartureMeans.Teleport => Resolution<ProvocationVerdict>.FromValue(new ProvocationVerdict(
                Provokes: false, $"the creature Teleports; {leaving}", locator, OwnerDecisions.None)),
            DepartureMeans.MovedWithoutItsOwn => Resolution<ProvocationVerdict>.FromValue(new ProvocationVerdict(
                Provokes: false,
                $"the creature is moved without using its movement, action, Bonus Action, or Reaction; {leaving}",
                locator,
                OwnerDecisions.None)),
            _ when leaving.DisengageTaken => Resolution<ProvocationVerdict>.FromValue(new ProvocationVerdict(
                Provokes: true,
                "the creature took the Disengage action on an earlier turn, and its protection covers your own movement for the "
                + $"rest of the turn it is taken on ('{MapEntries.DisengageAction.Id}' [{MapEntries.DisengageAction.Locator.Citation}]); {leaving}",
                locator,
                [followsTheGlossary])),
            _ => Resolution<ProvocationVerdict>.FromValue(new ProvocationVerdict(
                Provokes: true,
                $"the creature neither took the Disengage action, nor Teleported, nor was moved without using its own movement, action, Bonus Action, or Reaction; {leaving}",
                locator,
                OwnerDecisions.None)),
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
            // Derived from the avoidance rule's verdict, so it carries whatever that verdict relied on.
            return Resolution<OpportunityAttackOffer>.FromValue(
                OpportunityAttackOffer.None(verdict.Because, verdict.Authority, OwnerDecisions.InOrder(verdict.Decisions)));
        }

        if (sight.CannotSee)
        {
            return Resolution<OpportunityAttackOffer>.FromValue(OpportunityAttackOffer.None(
                $"an Opportunity Attack is made when a creature that you can see leaves your reach, and {sight}",
                MapEntries.OpportunityAttack.Locator,
                OwnerDecisions.None));
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
            MapEntries.OpportunityAttack.Locator,
            OwnerDecisions.InOrder(verdict?.Decisions ?? OwnerDecisions.None)));
    }
}
