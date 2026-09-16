using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RulesKernel.Randomness;
using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// A whole start of combat through the public surface, seeded: every participant rolls through
/// <see cref="EntryPoints.InitiativeRoll"/>, each tie's decider is read from
/// <see cref="EntryPoints.InitiativeTiesUncovered"/> and the tie breaks recorded, and the order comes
/// from <see cref="EntryPoints.InitiativeOrder"/>. The replay knows only the seed and the recorded
/// tie breaks.
/// </summary>
public class SeededInitiativeReplayTests
{
    private const ulong Seed = 20260915UL;

    /// <summary>
    /// The SHA-256 of the recorded combat start. A literal: if it changes, the engine rolls or orders
    /// this seed differently, which is a decision about the ruleset version, not a number to update
    /// until green.
    /// </summary>
    private const string RecordedReplaySha256 = "a8a8105d9220b2edd37fcd9d6a16a1d3691f9a336a2adc13afa31a517f6f7f33";

    private static readonly Combatant[] Participants =
    [
        // Under this seed the d20s are 8, 16, 8, 13, 8, 14, 18, so the modifiers make three ties: Bram and
        // Wolf at 18 (the GM's), Guide and Cleric at 11 (the players'), Aria and Orc at 10 (the GM's).
        Pc("Aria", 2), Pc("Bram", 2), Npc("Guide", 3), Monster("Goblin", 2), Monster("Orc", 2), Monster("Wolf", 4), Pc("Cleric", -7),
    ];

    [Fact]
    public void A_seeded_Initiative_replays_byte_for_byte_from_its_seed_and_its_recorded_tie_breaks()
    {
        var firstSource = new CountingSource(Pcg32.FromSeed(Seed, stream: 1));
        var (first, recorded) = Run(firstSource, decisions: null);
        byte[] bytes = Render(first.Rolls, first.Order);

        var replaySource = new CountingSource(Pcg32.FromSeed(Seed, stream: 1));
        var (replay, _) = Run(replaySource, recorded);

        Assert.Equal(bytes, Render(replay.Rolls, replay.Order));
        Assert.Equal(Participants.Length, firstSource.Drawn);
        Assert.Equal(firstSource.Drawn, replaySource.Drawn);
        Assert.NotEmpty(first.Order.TieBreaks);
        Assert.Equal(RecordedReplaySha256, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());

        // The generator is not ignored: another seed rolls differently.
        var (other, _) = Run(new CountingSource(Pcg32.FromSeed(Seed + 1, stream: 1)), decisions: null);
        Assert.NotEqual(first.Rolls.Rolls.Select(r => r.D20), other.Rolls.Rolls.Select(r => r.D20));

        // Comparable only under the same identity: ruleset srd-5.2.1-combat v6, PCG32, map 2.0.0.
        Assert.Equal("srd-5.2.1-combat", Ruleset.Identity.Ruleset.Id);
        Assert.Equal(6, Ruleset.Identity.Ruleset.Version);
        Assert.Equal(RulesKernel.Identity.RandomAlgorithmId.Pcg32SetSeq64XshRr32, Ruleset.Identity.RandomAlgorithm);
        Assert.StartsWith(
            "identity srd-5.2.1-combat v6 schema 1 map RulesFactory.Maps.Srd52Combat 2.0.0\n",
            Encoding.UTF8.GetString(bytes),
            StringComparison.Ordinal);
    }

    private static ((InitiativeRolls Rolls, TurnOrder Order) Result, TieBreaks Decisions) Run(IRandomSource source, TieBreaks? decisions)
    {
        var rolls = Value<InitiativeRolls>(EntryPoints.InitiativeRoll.Resolve(new InitiativeRollRequest
        {
            Participants = Participants,
            StatedBy = Gm,
            ScoreOption = Rolling,
            IdenticalCreatures = NoGroups,
            Source = source,
        }));

        // The first run decides every tie by a fixed policy, reverse listing order, as the decider
        // the rule assigns; the replay reads nothing but the recorded statements.
        decisions ??= TieBreaks.Of([.. Value<ImmutableArray<TieAssignment>>(
                EntryPoints.InitiativeTiesUncovered.Resolve(new InitiativeTiesUncoveredRequest { Counts = rolls.Counts }))
            .Select(tie => TieBreak.Ordered(tie.Decider, tie.Decider == TieDecider.Gm ? Gm : Table, [.. tie.Tied.Reverse()]))]);

        var order = Value<TurnOrder>(EntryPoints.InitiativeOrder.Resolve(
            new InitiativeOrderRequest(RuleRequest.Empty.Assert("initiative-ties", decisions)) { Counts = rolls.Counts }));
        return ((rolls, order), decisions);
    }

    /// <summary>The combat start as bytes: the identity, the map, every roll, every tie break, and the order.</summary>
    private static byte[] Render(InitiativeRolls rolls, TurnOrder order)
    {
        using var provenance = JsonDocument.Parse(EngineProvenance.ReadBytes());
        var map = provenance.RootElement.GetProperty("map");
        var identity = Ruleset.Identity;
        var text = new StringBuilder();
        text.Append($"identity {identity.Ruleset.Id} v{identity.Ruleset.Version} schema {identity.ReplaySchema.Version} ")
            .Append($"map {map.GetProperty("packageId").GetString()} {map.GetProperty("version").GetString()}\n");
        foreach (var baseline in identity.SourceBaselines)
        {
            text.Append($"baseline {baseline.SourceId} {baseline.ContentHash} {baseline.HashDerivation}\n");
        }

        text.Append($"generator {identity.RandomAlgorithm?.Name} seed {Seed} stream 1\n")
            .Append($"statements {rolls.ScoreOption}; {rolls.IdenticalCreatures}; participants stated by {rolls.StatedBy}\n");
        foreach (var roll in rolls.Rolls)
        {
            text.Append($"roll {roll} [{rolls.Authority}]\n");
        }

        foreach (var tieBreak in order.TieBreaks)
        {
            text.Append($"tie {tieBreak}\n");
        }

        text.Append($"order {string.Join(" ", order.TurnsInRound(1))} [{order.Authority}]\n");
        return Encoding.UTF8.GetBytes(text.ToString());
    }
}
