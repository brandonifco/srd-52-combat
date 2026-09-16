using Srd52Combat.Attacks;

namespace Srd52Combat.Tests;

/// <summary>What the attack tests share: the statements a caller owes, stated by the GM.</summary>
internal static class AttackFixtures
{
    internal static readonly IncapacitatedStatement NotIncapacitated = IncapacitatedStatement.IsNot(Fixtures.Gm);

    internal static readonly IncapacitatedStatement Incapacitated = IncapacitatedStatement.Is(Fixtures.Gm);

    internal static readonly AttackDamageRules OrdinaryDamage = AttackDamageRules.Ordinary(Fixtures.Gm);

    internal static readonly ReactionAvailability HasReaction = ReactionAvailability.Has(Fixtures.Gm);

    internal static readonly TargetVisibilityStatement Sees = TargetVisibilityStatement.Seen(Fixtures.Gm);

    /// <summary>A Longbow's ranges, the corpus's own example of an attack with two ranges.</summary>
    internal static readonly TwoRanges Longbow = new(150, 600);
}
