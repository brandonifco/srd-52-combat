using RulesKernel.Randomness;
using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Xunit;

namespace Srd52Combat.Tests;

/// <summary>What the Initiative tests share: statements, a counting generator, and unwrapping.</summary>
internal static class Fixtures
{
    internal const string Gm = "the GM";

    internal const string Table = "the players";

    internal static readonly InitiativeScoreOptionStatement Rolling = InitiativeScoreOptionStatement.Rolling(Gm);

    internal static readonly IdenticalCreaturesStatement NoGroups = IdenticalCreaturesStatement.None(Gm);

    internal static Combatant Pc(string id, int modifier = 0, D20Mode roll = D20Mode.Straight) =>
        new(id, CombatantKind.PlayerCharacter, modifier, roll);

    internal static Combatant Npc(string id, int modifier = 0) =>
        new(id, CombatantKind.NonPlayerCharacter, modifier, D20Mode.Straight);

    internal static Combatant Monster(string id, int modifier = 0, D20Mode roll = D20Mode.Straight) =>
        new(id, CombatantKind.Monster, modifier, roll);

    internal static InitiativeCount Count(string id, CombatantKind kind, int initiative) => new(id, kind, initiative);

    internal static T Value<T>(Resolution<object> resolution) =>
        Assert.IsType<T>(Assert.IsType<Resolution<object>.Resolved>(resolution).Value);

    internal static UnresolvedResult Declined(Resolution<object> resolution) =>
        Assert.IsType<Resolution<object>.Unresolved>(resolution).Result;
}

/// <summary>A kernel <see cref="IRandomSource"/> that counts the raw values drawn through it.</summary>
internal sealed class CountingSource(IRandomSource inner) : IRandomSource
{
    public int Drawn { get; private set; }

    public uint NextUInt32()
    {
        Drawn++;
        return inner.NextUInt32();
    }
}
