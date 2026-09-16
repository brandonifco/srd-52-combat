using RulesKernel.Randomness;
using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// <c>initiative-roll</c>, "Combat / Initiative / p. 13", through <see cref="EntryPoints.InitiativeRoll"/>
/// only: one d20 per participant from the seeded generator, the statements that change what is
/// drawn, and the gate outside the slice (rules-factory decision 0021).
/// </summary>
public class InitiativeRollEntryPointTests
{
    private static Resolution<object> Roll(
        IReadOnlyList<Combatant> participants,
        IRandomSource source,
        InitiativeScoreOptionStatement? scoreOption = null,
        IdenticalCreaturesStatement? identical = null) =>
        EntryPoints.InitiativeRoll.Resolve(new InitiativeRollRequest
        {
            Participants = participants,
            StatedBy = Gm,
            ScoreOption = scoreOption ?? Rolling,
            IdenticalCreatures = identical ?? NoGroups,
            Source = source,
        });

    [Fact]
    public void Every_participant_rolls_one_d20_plus_their_Dexterity_check_modifier_citing_page_13()
    {
        Combatant[] participants = [Pc("Aria", 3), Monster("Goblin", 2), Npc("Guide", -1), Pc("Bram", 0)];
        var source = new CountingSource(Pcg32.FromSeed(7UL, stream: 1));

        var rolls = Value<InitiativeRolls>(Roll(participants, source));

        // One bounded d20 per participant, in the order given: the same generator, drawn the same
        // way by the caller, gives the same faces.
        var expected = Pcg32.FromSeed(7UL, stream: 1);
        Assert.Equal(participants.Select(p => p.Id), rolls.Rolls.Select(r => r.CombatantId));
        foreach (var (roll, participant) in rolls.Rolls.Zip(participants))
        {
            Assert.Equal(UniformInt.InRange(expected, 1, 20), roll.D20);
            Assert.Equal(participant.DexterityCheckModifier, roll.Modifier);
            Assert.Equal(roll.D20 + participant.DexterityCheckModifier, roll.Total);
            Assert.Equal(new InitiativeCount(participant.Id, participant.Kind, roll.Total), roll.Count);
        }

        Assert.Equal(participants.Length, source.Drawn);
        Assert.Equal("srd-5.2.1", rolls.Authority.SourceId);
        Assert.Equal("Combat / Initiative / p. 13", rolls.Authority.Citation);
        Assert.Equal(EntryPoints.InitiativeRoll.Registered.Locator, rolls.Authority);
        Assert.Same(Rolling, rolls.ScoreOption);
        Assert.Same(NoGroups, rolls.IdenticalCreatures);
        Assert.Equal(Gm, rolls.StatedBy);
    }

    [Fact]
    public void Faces_cover_one_to_twenty_and_nothing_else()
    {
        var source = Pcg32.FromSeed(20260915UL, stream: 3);
        var faces = new HashSet<int>();
        for (int i = 0; i < 400; i++)
        {
            faces.Add(Value<InitiativeRolls>(Roll([Pc("Aria")], source)).Rolls[0].D20);
        }

        Assert.Equal(Enumerable.Range(1, 20), faces.Order());
    }

    [Fact]
    public void While_the_GM_uses_Initiative_scores_nothing_is_rolled_and_it_declines_citing_page_184()
    {
        var source = new CountingSource(Pcg32.FromSeed(7UL, stream: 1));
        var scores = InitiativeScoreOptionStatement.ScoresInUse(Gm);

        var declined = Declined(Roll([Pc("Aria", 3), Monster("Goblin", 2)], source, scoreOption: scores));

        Assert.Equal(UnresolvedReason.OutsideCurrentScope, declined.Reason);
        Assert.Equal("Rules Glossary / Initiative / p. 184", declined.Locator.Citation);
        Assert.Equal(EntryPoints.InitiativeScoreOption.Registered.Locator, declined.Locator);
        Assert.Contains("'initiative-roll'", declined.Attempted, StringComparison.Ordinal);
        Assert.Contains(scores.ToString(), declined.Attempted, StringComparison.Ordinal);
        Assert.Equal(0, source.Drawn);
    }

    [Fact]
    public void A_group_of_identical_creatures_declines_citing_group_initiative_without_drawing()
    {
        var source = new CountingSource(Pcg32.FromSeed(7UL, stream: 1));
        var grouped = IdenticalCreaturesStatement.Grouped(Gm, ["Goblin 1", "Goblin 2"]);

        var declined = Declined(Roll([Pc("Aria", 3), Monster("Goblin 1", 2), Monster("Goblin 2", 2)], source, identical: grouped));

        // group-initiative is built now, and declines: what makes creatures a group of identical
        // creatures is not stated, so how many d20s the combat throws is not fixed (decision 0005).
        Assert.Equal(UnresolvedReason.RequiresInterpretation, declined.Reason);
        Assert.Equal(EntryPoints.GroupInitiative.Registered.Locator, declined.Locator);
        Assert.Contains("'group-initiative'", declined.Attempted, StringComparison.Ordinal);
        Assert.Contains(grouped.ToString(), declined.Attempted, StringComparison.Ordinal);
        Assert.Equal(0, source.Drawn);
    }

    [Theory]
    [InlineData(D20Mode.Advantage)]
    [InlineData(D20Mode.Disadvantage)]
    public void A_roll_with_Advantage_or_Disadvantage_declines_citing_page_7_without_drawing(D20Mode mode)
    {
        var source = new CountingSource(Pcg32.FromSeed(7UL, stream: 1));

        var declined = Declined(Roll([Pc("Aria", 3), Monster("Goblin", 2, mode)], source));

        Assert.Equal(UnresolvedReason.OutsideCurrentScope, declined.Reason);
        Assert.Equal("Playing the Game / Advantage/Disadvantage / p. 7", declined.Locator.Citation);
        Assert.Equal(EntryPoints.AdvantageDisadvantage.Registered.Locator, declined.Locator);
        Assert.Contains($"Goblin's roll has {mode}", declined.Attempted, StringComparison.Ordinal);
        Assert.Equal(0, source.Drawn);
    }

    [Fact]
    public void A_roll_with_both_Advantage_and_Disadvantage_declines_citing_page_7_without_drawing()
    {
        // Map 2.0.0's draws counts one d20 for this roll, because the two cancel. The cancelling is
        // advantage-disadvantage's rule, scope: out, so the engine declines it (decision 0002).
        var source = new CountingSource(Pcg32.FromSeed(7UL, stream: 1));

        var declined = Declined(Roll([Pc("Aria", 3, D20Mode.AdvantageAndDisadvantage), Monster("Goblin", 2)], source));

        Assert.Equal(UnresolvedReason.OutsideCurrentScope, declined.Reason);
        Assert.Equal(EntryPoints.AdvantageDisadvantage.Registered.Locator, declined.Locator);
        Assert.Contains("Aria's roll has AdvantageAndDisadvantage", declined.Attempted, StringComparison.Ordinal);
        Assert.Equal(0, source.Drawn);
    }

    [Fact]
    public void A_statement_left_out_is_refused_never_inferred()
    {
        var source = Pcg32.FromSeed(7UL, stream: 1);
        Combatant[] participants = [Pc("Aria", 3)];

        var noScoreOption = Assert.Throws<ArgumentException>(() => EntryPoints.InitiativeRoll.Resolve(new InitiativeRollRequest
        {
            Participants = participants,
            StatedBy = Gm,
            IdenticalCreatures = NoGroups,
            Source = source,
        }));
        Assert.Equal("ScoreOption", noScoreOption.ParamName);

        var noGroups = Assert.Throws<ArgumentException>(() => EntryPoints.InitiativeRoll.Resolve(new InitiativeRollRequest
        {
            Participants = participants,
            StatedBy = Gm,
            ScoreOption = Rolling,
            Source = source,
        }));
        Assert.Equal("IdenticalCreatures", noGroups.ParamName);

        Assert.Throws<ArgumentException>(() => new Combatant("Aria", CombatantKind.PlayerCharacter, 3, default));
        Assert.Throws<ArgumentException>(() => new Combatant("Aria", default, 3, D20Mode.Straight));
    }
}
