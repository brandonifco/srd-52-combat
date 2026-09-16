using RulesKernel.Randomness;
using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Requests;
using Srd52Combat.Turn;
using Xunit;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// <c>surprise-disadvantage</c> and <c>group-initiative</c>, both "Combat / Initiative / p. 13",
/// through their entry points only.
/// </summary>
public class SurpriseAndGroupEntryPointTests
{
    private static readonly Combatant[] Participants = [Pc("Aria", 2), Monster("Goblin", 2), Monster("Orc", 2), Npc("Guide", 1)];

    private static Resolution<object> Surprise(bool surprised, InitiativeScoreOptionStatement? scoreOption = null) =>
        EntryPoints.SurpriseDisadvantage.Resolve(new SurpriseDisadvantageRequest
        {
            Surprised = new SurprisedStatement("Aria", surprised, Gm),
            ScoreOption = scoreOption ?? Rolling,
        });

    private static Resolution<object> Groups(IdenticalCreaturesStatement groups) =>
        EntryPoints.GroupInitiative.Resolve(new GroupInitiativeRequest
        {
            Participants = Participants,
            StatedBy = Gm,
            IdenticalCreatures = groups,
        });

    [Fact]
    public void A_surprised_combatant_has_Disadvantage_on_their_Initiative_roll_citing_page_13()
    {
        var effect = Value<SurpriseEffect>(Surprise(surprised: true));

        Assert.True(effect.DisadvantageOnInitiativeRoll);
        Assert.Equal(D20Mode.Disadvantage, effect.InitiativeRoll);
        Assert.Equal("Aria", effect.CombatantId);
        Assert.Equal(Gm, effect.Statement.StatedBy);
        Assert.Equal("Combat / Initiative / p. 13", effect.Authority.Citation);
        Assert.Equal(EntryPoints.SurpriseDisadvantage.Registered.Locator, effect.Authority);
    }

    [Fact]
    public void A_combatant_who_is_not_surprised_gets_nothing_from_this_rule()
    {
        var effect = Value<SurpriseEffect>(Surprise(surprised: false));

        Assert.False(effect.DisadvantageOnInitiativeRoll);

        // Not Straight: this rule speaks of surprise, and a roll's mode comes from every source.
        Assert.Null(effect.InitiativeRoll);
    }

    [Fact]
    public void While_the_GM_uses_Initiative_scores_surprise_declines_citing_page_184()
    {
        var declined = Declined(Surprise(surprised: true, InitiativeScoreOptionStatement.ScoresInUse(Gm)));

        Assert.Equal(UnresolvedReason.OutsideCurrentScope, declined.Reason);
        Assert.Equal("Rules Glossary / Initiative / p. 184", declined.Locator.Citation);
        Assert.Equal(EntryPoints.InitiativeScoreOption.Registered.Locator, declined.Locator);
        Assert.Contains("surprise-disadvantage", declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void Surprise_is_stated_never_inferred_and_so_is_the_Initiative_score_option()
    {
        var surprised = Assert.Throws<ArgumentException>(() =>
            EntryPoints.SurpriseDisadvantage.Resolve(new SurpriseDisadvantageRequest { ScoreOption = Rolling }));
        Assert.Equal("Surprised", surprised.ParamName);

        var scoreOption = Assert.Throws<ArgumentException>(() =>
            EntryPoints.SurpriseDisadvantage.Resolve(new SurpriseDisadvantageRequest
            {
                Surprised = new SurprisedStatement("Aria", IsSurprised: true, Gm),
            }));
        Assert.Equal("ScoreOption", scoreOption.ParamName);
    }

    [Fact]
    public void The_Disadvantage_surprise_gives_is_the_roll_mode_the_caller_states_to_initiative_roll()
    {
        var effect = Value<SurpriseEffect>(Surprise(surprised: true));
        var source = new CountingSource(Pcg32.FromSeed(7UL, stream: 1));

        // The roll mode this rule gives is what the caller states; how Disadvantage resolves is
        // outside the slice, so the roll itself declines and draws nothing.
        var declined = Declined(EntryPoints.InitiativeRoll.Resolve(new InitiativeRollRequest
        {
            Participants = [new Combatant("Aria", CombatantKind.PlayerCharacter, 2, effect.InitiativeRoll!.Value)],
            StatedBy = Gm,
            ScoreOption = Rolling,
            IdenticalCreatures = NoGroups,
            Source = source,
        }));

        Assert.Equal(UnresolvedReason.OutsideCurrentScope, declined.Reason);
        Assert.Equal("Playing the Game / Advantage/Disadvantage / p. 7", declined.Locator.Citation);
        Assert.Equal(0, source.Drawn);
    }

    [Fact]
    public void The_GM_rolls_for_the_monsters_when_no_group_of_identical_creatures_is_stated()
    {
        var rolls = Value<GroupInitiativeRolls>(Groups(NoGroups));

        Assert.Equal(new[] { "Goblin", "Orc" }, rolls.MonstersTheGmRollsFor);
        Assert.Equal(0, rolls.Draws);
        Assert.Equal(Gm, rolls.StatedBy);
        Assert.Equal("Combat / Initiative / p. 13", rolls.Authority.Citation);
        Assert.Equal(EntryPoints.GroupInitiative.Registered.Locator, rolls.Authority);
    }

    [Fact]
    public void A_stated_group_of_identical_creatures_rolls_one_d20_each_naming_the_owners_ruling()
    {
        var group = IdenticalCreaturesStatement.Grouped(Gm, ["Goblin", "Orc"]);

        var rolls = Value<GroupInitiativeRolls>(Groups(group));

        // The statement is recorded and changes nothing: this rule still gives the GM no roll.
        Assert.Equal(new[] { "Goblin", "Orc" }, rolls.MonstersTheGmRollsFor);
        Assert.Equal(0, rolls.Draws);
        Assert.Same(group, rolls.Groups);

        // That the group takes no single roll is Brandon's ruling, not the corpus's silence.
        var owners = Assert.Single(rolls.Rulings);
        Assert.Equal("group-initiative/no-grouping", owners.Id);
        Assert.Equal("group-initiative", owners.EntryId);
        Assert.Equal("Brandon", owners.RuledBy);
        Assert.Equal(new DateOnly(2026, 9, 15), owners.RuledOn);
        Assert.Equal("docs/decisions/0007-brandons-rulings-on-six-open-questions-are-ruleset-version-seven.md", owners.Record);
        Assert.Contains("group of identical creatures", owners.Span, StringComparison.Ordinal);

        // The same group put to initiative-roll now rolls one d20 per participant, naming the ruling.
        var source = new CountingSource(Pcg32.FromSeed(7UL, stream: 1));
        var rolled = Value<InitiativeRolls>(EntryPoints.InitiativeRoll.Resolve(new InitiativeRollRequest
        {
            Participants = Participants,
            StatedBy = Gm,
            ScoreOption = Rolling,
            IdenticalCreatures = group,
            Source = source,
        }));

        Assert.Equal(Participants.Length, rolled.Rolls.Length);
        Assert.Equal(Participants.Length, source.Drawn);
        Assert.Equal(owners, Assert.Single(rolled.Rulings));

        // Each member of the "group" has its own d20, which is what the ruling says.
        Assert.Equal(
            Participants.Select(p => p.Id),
            rolled.Rolls.Select(r => r.CombatantId));
    }

    [Fact]
    public void The_owners_ruling_is_named_only_where_a_group_was_stated()
    {
        Assert.Empty(Value<GroupInitiativeRolls>(Groups(NoGroups)).Rulings);

        var source = new CountingSource(Pcg32.FromSeed(7UL, stream: 1));
        var rolled = Value<InitiativeRolls>(EntryPoints.InitiativeRoll.Resolve(new InitiativeRollRequest
        {
            Participants = Participants,
            StatedBy = Gm,
            ScoreOption = Rolling,
            IdenticalCreatures = NoGroups,
            Source = source,
        }));

        Assert.Empty(rolled.Rulings);
        Assert.Equal(Participants.Length, source.Drawn);

        // The ruling the overlay holds is the one the engine names, and the registry has it once.
        Assert.Single(OwnerRulings.All.Where(r => r.EntryId == "group-initiative"));
    }

    [Fact]
    public void A_group_naming_someone_who_is_not_a_participant_is_refused()
    {
        var unknown = Assert.Throws<ArgumentException>(() => Groups(IdenticalCreaturesStatement.Grouped(Gm, ["Goblin", "Wolf"])));
        Assert.Equal("identicalCreatures", unknown.ParamName);

        var missing = Assert.Throws<ArgumentException>(() =>
            EntryPoints.GroupInitiative.Resolve(new GroupInitiativeRequest { Participants = Participants, StatedBy = Gm }));
        Assert.Equal("IdenticalCreatures", missing.ParamName);
    }
}
