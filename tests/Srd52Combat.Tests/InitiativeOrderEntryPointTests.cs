using System.Collections.Immutable;
using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// <c>initiative-order</c>, <c>initiative-ties</c> and <c>initiative-ties-uncovered</c>, all
/// "Combat / Initiative / p. 13", through their entry points only.
/// </summary>
public class InitiativeOrderEntryPointTests
{
    private static RuleRequest Asserting(params TieBreak[] breaks) =>
        RuleRequest.Empty.Assert("initiative-ties", TieBreaks.Of(breaks));

    private static Resolution<object> Order(InitiativeCount[] counts, RuleRequest? assertions = null) =>
        EntryPoints.InitiativeOrder.Resolve(new InitiativeOrderRequest(assertions ?? RuleRequest.Empty) { Counts = counts });

    private static Resolution<object> Ties(InitiativeCount[] counts, RuleRequest assertions) =>
        EntryPoints.InitiativeTies.Resolve(new InitiativeTiesRequest(assertions) { Counts = counts });

    private static Resolution<object> Assigned(InitiativeCount[] counts) =>
        EntryPoints.InitiativeTiesUncovered.Resolve(new InitiativeTiesUncoveredRequest { Counts = counts });

    [Fact]
    public void Combatants_act_from_highest_to_lowest_Initiative_the_same_in_every_round()
    {
        InitiativeCount[] counts =
        [
            Count("Aria", CombatantKind.PlayerCharacter, 12),
            Count("Goblin", CombatantKind.Monster, 19),
            Count("Guide", CombatantKind.NonPlayerCharacter, 3),
            Count("Bram", CombatantKind.PlayerCharacter, 15),
        ];

        // No tie, so no tie break is demanded: the request asserts nothing.
        var order = Value<TurnOrder>(Order(counts));

        string[] expected = ["Goblin", "Bram", "Aria", "Guide"];
        Assert.Equal(expected, order.Order.Select(c => c.CombatantId));
        Assert.Equal(expected, order.TurnsInRound(1));
        Assert.Equal(expected, order.TurnsInRound(2));
        Assert.Equal(expected, order.TurnsInRound(10));
        Assert.Empty(order.TieBreaks);
        Assert.Equal("Combat / Initiative / p. 13", order.Authority.Citation);
        Assert.Equal(EntryPoints.InitiativeOrder.Registered.Locator, order.Authority);
    }

    [Fact]
    public void A_tie_is_ordered_as_its_decider_stated_and_the_statement_is_recorded()
    {
        InitiativeCount[] counts =
        [
            Count("Goblin", CombatantKind.Monster, 14),
            Count("Aria", CombatantKind.PlayerCharacter, 9),
            Count("Orc", CombatantKind.Monster, 14),
            Count("Bram", CombatantKind.PlayerCharacter, 9),
            Count("Wolf", CombatantKind.Monster, 20),
        ];
        var monsters = TieBreak.Ordered(TieDecider.Gm, Gm, "Orc", "Goblin");
        var characters = TieBreak.Ordered(TieDecider.Players, Table, "Bram", "Aria");

        var order = Value<TurnOrder>(Order(counts, Asserting(characters, monsters)));

        Assert.Equal(["Wolf", "Orc", "Goblin", "Bram", "Aria"], order.Order.Select(c => c.CombatantId));
        Assert.Collection(order.TieBreaks, b => Assert.Same(monsters, b), b => Assert.Same(characters, b));
        Assert.Equal(Table, order.TieBreaks[1].StatedBy);

        // The other way round is followed just as faithfully: nothing about the tie is the engine's.
        var reversed = Value<TurnOrder>(Order(counts, Asserting(
            TieBreak.Ordered(TieDecider.Players, Table, "Aria", "Bram"),
            TieBreak.Ordered(TieDecider.Gm, Gm, "Goblin", "Orc"))));
        Assert.Equal(["Wolf", "Goblin", "Orc", "Aria", "Bram"], reversed.Order.Select(c => c.CombatantId));
    }

    [Fact]
    public void A_tie_with_no_tie_break_stated_is_refused_never_inferred()
    {
        InitiativeCount[] counts = [Count("Aria", CombatantKind.PlayerCharacter, 9), Count("Bram", CombatantKind.PlayerCharacter, 9)];

        var order = Assert.Throws<AssertionRequiredException>(() => Order(counts));
        Assert.Equal("initiative-ties", order.EntryId);

        var ties = Assert.Throws<AssertionRequiredException>(() => Ties(counts, RuleRequest.Empty));
        Assert.Equal("initiative-ties", ties.EntryId);
    }

    [Fact]
    public void The_tie_breaks_resolve_checked_against_the_ties_and_nothing_is_demanded_without_one()
    {
        InitiativeCount[] counts =
        [
            Count("Goblin", CombatantKind.Monster, 11),
            Count("Aria", CombatantKind.PlayerCharacter, 11),
            Count("Bram", CombatantKind.PlayerCharacter, 5),
        ];
        var mixed = TieBreak.Ordered(TieDecider.Gm, Gm, "Aria", "Goblin");

        Assert.Same(mixed, Assert.Single(Value<ImmutableArray<TieBreak>>(Ties(counts, Asserting(mixed)))));
        Assert.Empty(Value<ImmutableArray<TieBreak>>(Ties([Count("Aria", CombatantKind.PlayerCharacter, 9)], RuleRequest.Empty)));
    }

    public static TheoryData<string, TieBreak> WrongTieBreaks => new()
    {
        // A tie among characters is the players' to decide, not the GM's.
        { "the Players's to decide", TieBreak.Ordered(TieDecider.Gm, Gm, "Bram", "Aria") },

        // A tie between a monster and a player character is the GM's.
        { "the Gm's to decide", TieBreak.Ordered(TieDecider.Players, Table, "Orc", "Goblin", "Cleric") },

        // A tie break must order exactly the tied combatants.
        { "needs exactly one tie break", TieBreak.Ordered(TieDecider.Players, Table, "Bram", "Aria", "Wolf") },

        // A tie break for combatants who are not tied is not a statement about any tie.
        { "no tie exists", TieBreak.Ordered(TieDecider.Gm, Gm, "Wolf", "Orc") },
    };

    [Theory]
    [MemberData(nameof(WrongTieBreaks))]
    public void A_tie_break_by_the_wrong_decider_or_of_other_combatants_is_refused(string message, TieBreak wrong)
    {
        InitiativeCount[] counts =
        [
            Count("Aria", CombatantKind.PlayerCharacter, 9),
            Count("Bram", CombatantKind.PlayerCharacter, 9),
            Count("Orc", CombatantKind.Monster, 14),
            Count("Goblin", CombatantKind.Monster, 14),
            Count("Cleric", CombatantKind.PlayerCharacter, 14),
            Count("Wolf", CombatantKind.Monster, 2),
        ];
        var right = new[]
        {
            TieBreak.Ordered(TieDecider.Players, Table, "Aria", "Bram"),
            TieBreak.Ordered(TieDecider.Gm, Gm, "Orc", "Cleric", "Goblin"),
        };
        var stated = right.Where(b => !b.Order.ToHashSet().SetEquals(wrong.Order.Where(id => id != "Wolf"))).Append(wrong).ToArray();

        var refused = Assert.Throws<ArgumentException>(() => Ties(counts, Asserting(stated)));
        Assert.Contains(message, refused.Message, StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => Order(counts, Asserting(stated)));
        _ = Value<TurnOrder>(Order(counts, Asserting(right)));
    }

    public static TheoryData<CombatantKind[], TieDecider> Assignments => new()
    {
        { [CombatantKind.Monster, CombatantKind.Monster], TieDecider.Gm },
        { [CombatantKind.PlayerCharacter, CombatantKind.PlayerCharacter], TieDecider.Players },
        { [CombatantKind.PlayerCharacter, CombatantKind.NonPlayerCharacter], TieDecider.Players },
        { [CombatantKind.NonPlayerCharacter, CombatantKind.NonPlayerCharacter], TieDecider.Players },
        { [CombatantKind.Monster, CombatantKind.PlayerCharacter], TieDecider.Gm },
        { [CombatantKind.PlayerCharacter, CombatantKind.Monster, CombatantKind.PlayerCharacter], TieDecider.Gm },
    };

    [Theory]
    [MemberData(nameof(Assignments))]
    public void Ties_among_monsters_or_between_monsters_and_player_characters_are_the_GMs_and_among_characters_the_players(CombatantKind[] kinds, TieDecider decider)
    {
        var counts = kinds.Select((kind, i) => Count($"c{i}", kind, 10)).Append(Count("alone", CombatantKind.Monster, 4)).ToArray();

        var assignment = Assert.Single(Value<ImmutableArray<TieAssignment>>(Assigned(counts)));

        Assert.Equal(decider, assignment.Decider);
        Assert.Equal(10, assignment.Initiative);
        Assert.Equal(kinds.Select((_, i) => $"c{i}"), assignment.Tied);
        Assert.Equal("Combat / Initiative / p. 13", assignment.Authority.Citation);
    }

    [Fact]
    public void A_tie_between_a_monster_and_a_non_player_character_declines_citing_page_13()
    {
        InitiativeCount[] counts =
        [
            Count("Aria", CombatantKind.PlayerCharacter, 18),
            Count("Goblin", CombatantKind.Monster, 12),
            Count("Guide", CombatantKind.NonPlayerCharacter, 12),
        ];

        // Whatever the caller asserts, nobody is assigned to decide it, so the engine does not.
        var anyOrder = Asserting(TieBreak.Ordered(TieDecider.Gm, Gm, "Guide", "Goblin"));
        foreach (var resolution in new[] { Assigned(counts), Order(counts, anyOrder), Ties(counts, anyOrder) })
        {
            var declined = Declined(resolution);
            Assert.Equal(UnresolvedReason.RequiresInterpretation, declined.Reason);
            Assert.Equal("srd-5.2.1", declined.Locator.SourceId);
            Assert.Equal("Combat / Initiative / p. 13", declined.Locator.Citation);
            Assert.Equal(EntryPoints.InitiativeTiesUncovered.Registered.Locator, declined.Locator);
            Assert.Contains("'initiative-ties-uncovered'", declined.Attempted, StringComparison.Ordinal);
            Assert.Contains("Goblin (Monster), Guide (NonPlayerCharacter)", declined.Attempted, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Players_who_state_they_did_not_agree_decline_citing_page_13_with_the_statement_recorded()
    {
        InitiativeCount[] counts =
        [
            Count("Aria", CombatantKind.PlayerCharacter, 9),
            Count("Bram", CombatantKind.PlayerCharacter, 9),
            Count("Goblin", CombatantKind.Monster, 3),
        ];
        var disagreement = TieBreak.PlayersDisagree(Table, "Aria", "Bram");

        foreach (var resolution in new[] { Order(counts, Asserting(disagreement)), Ties(counts, Asserting(disagreement)) })
        {
            var declined = Declined(resolution);
            Assert.Equal(UnresolvedReason.RequiresInterpretation, declined.Reason);
            Assert.Equal(EntryPoints.InitiativeTiesUncovered.Registered.Locator, declined.Locator);
            Assert.Contains("'initiative-ties-uncovered'", declined.Attempted, StringComparison.Ordinal);
            Assert.Contains(disagreement.ToString(), declined.Attempted, StringComparison.Ordinal);
        }
    }
}
