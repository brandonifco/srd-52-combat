using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Requests;
using Srd52Combat.Turn;
using Xunit;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// <c>combat-steps</c> ("Combat / Combat Step by Step / p. 13") and <c>next-round</c> ("Combat / The
/// Order of Combat / p. 13"), through their entry points only.
/// </summary>
public class RoundEntryPointTests
{
    private static readonly InitiativeCount[] Counts =
    [
        Count("Aria", CombatantKind.PlayerCharacter, 17),
        Count("Goblin", CombatantKind.Monster, 12),
        Count("Bram", CombatantKind.PlayerCharacter, 9),
    ];

    private static TurnOrder Order() =>
        Value<TurnOrder>(EntryPoints.InitiativeOrder.Resolve(new InitiativeOrderRequest { Counts = Counts }));

    private static Resolution<object> NextRound(
        int round,
        string[] turnsTaken,
        SideDefeatedStatement? defeat = null,
        SidesAgreementStatement? agreement = null) =>
        EntryPoints.NextRound.Resolve(new NextRoundRequest
        {
            Round = round,
            Order = Order().TurnsInRound(round),
            TurnsTaken = turnsTaken,
            Defeat = defeat ?? SideDefeatedStatement.Neither(Gm),
            Agreement = agreement ?? SidesAgreementStatement.NotAgreed(Gm),
        });

    [Fact]
    public void Combat_unfolds_in_three_steps_positions_Initiative_then_turns_in_Initiative_order()
    {
        var positions = PositionsStatement.Of(
            Gm,
            new Position("Aria", "20 feet north of the goblin"),
            new Position("Goblin", "behind the crates"),
            new Position("Bram", "at the door"));

        var combat = Value<CombatStepByStep>(EntryPoints.CombatSteps.Resolve(
            new CombatStepsRequest { Positions = positions, Order = Order() }));

        Assert.Equal(new[] { 1, 2, 3 }, combat.Steps.Select(s => s.Number));
        Assert.Equal(
            new[] { "Establish Positions", "Roll Initiative", "Take Turns" },
            combat.Steps.Select(s => s.Name));
        Assert.Same(positions, combat.Positions);
        Assert.Contains(Gm, combat.Steps[0].What, StringComparison.Ordinal);

        // Step 3: each participant takes a turn in Initiative order, the same order in every round.
        string[] expected = ["Aria", "Goblin", "Bram"];
        Assert.Equal(expected, combat.TurnsInRound(1));
        Assert.Equal(expected, combat.TurnsInRound(2));
        Assert.Equal("Combat / Combat Step by Step / p. 13", combat.Authority.Citation);
        Assert.Equal(EntryPoints.CombatSteps.Registered.Locator, combat.Authority);
    }

    [Fact]
    public void No_turn_is_taken_before_Initiative_is_rolled()
    {
        var positions = PositionsStatement.Of(Gm, new Position("Aria", "at the door"));

        // Step 3's turns are step 2's order: with no Initiative order there is no sequence, and the
        // engine infers none.
        var missing = Assert.Throws<ArgumentException>(() =>
            EntryPoints.CombatSteps.Resolve(new CombatStepsRequest { Positions = positions }));
        Assert.Equal("Order", missing.ParamName);

        var noPositions = Assert.Throws<ArgumentException>(() =>
            EntryPoints.CombatSteps.Resolve(new CombatStepsRequest { Order = Order() }));
        Assert.Equal("Positions", noPositions.ParamName);
    }

    [Fact]
    public void The_fight_continues_to_the_next_round_once_everyone_has_taken_a_turn_and_neither_side_is_defeated()
    {
        var outcome = Value<NextRoundOutcome>(NextRound(1, ["Aria", "Goblin", "Bram"]));

        Assert.True(outcome.RoundOver);
        Assert.True(outcome.Continues);
        Assert.Equal(2, outcome.Next);
        Assert.Empty(outcome.Waiting);
        Assert.Equal("Combat / The Order of Combat / p. 13", outcome.Authority.Citation);
        Assert.Equal(EntryPoints.NextRound.Registered.Locator, outcome.Authority);

        // And so on: the round after round 4 is round 5.
        Assert.Equal(5, Value<NextRoundOutcome>(NextRound(4, ["Bram", "Aria", "Goblin"])).Next);
    }

    [Fact]
    public void Before_everyone_has_taken_a_turn_the_round_is_not_over()
    {
        var outcome = Value<NextRoundOutcome>(NextRound(1, ["Aria"]));

        Assert.False(outcome.RoundOver);
        Assert.False(outcome.Continues);
        Assert.Null(outcome.Next);
        Assert.Equal(new[] { "Goblin", "Bram" }, outcome.Waiting);

        // Even with a side defeated, this rule says only that the round is not over.
        var defeated = Value<NextRoundOutcome>(NextRound(1, [], SideDefeatedStatement.Defeated(Gm, "the goblins")));
        Assert.False(defeated.RoundOver);
        Assert.Null(defeated.Next);

        // A turn by someone outside the order, or a second turn in the round, is refused.
        Assert.Throws<ArgumentException>(() => NextRound(1, ["Aria", "Aria"]));
        Assert.Throws<ArgumentException>(() => NextRound(1, ["Nobody"]));
    }

    [Fact]
    public void A_defeated_side_ends_the_round_without_another_following()
    {
        var outcome = Value<NextRoundOutcome>(NextRound(
            3,
            ["Aria", "Goblin", "Bram"],
            SideDefeatedStatement.Defeated(Gm, "the goblins")));

        Assert.True(outcome.RoundOver);
        Assert.False(outcome.Continues);
        Assert.Null(outcome.Next);
        Assert.Contains("the goblins is defeated", outcome.Why, StringComparison.Ordinal);
    }

    [Fact]
    public void Both_sides_agreeing_with_neither_defeated_ends_the_combat_naming_the_owners_ruling()
    {
        var outcome = Value<NextRoundOutcome>(NextRound(
            2,
            ["Aria", "Goblin", "Bram"],
            SideDefeatedStatement.Neither(Gm),
            SidesAgreementStatement.Agreed(Gm)));

        Assert.True(outcome.RoundOver);
        Assert.False(outcome.Continues);
        Assert.Null(outcome.Next);
        Assert.Contains("combat-end", outcome.Why, StringComparison.Ordinal);
        Assert.Equal(EntryPoints.NextRound.Registered.Locator, outcome.Authority);

        // p. 13 continues the fight and p. 14 ends the combat. Which governs is Brandon's, not the
        // corpus's, and the answer says so.
        var owners = Assert.Single(outcome.Rulings);
        Assert.Equal("next-round/agreement-ends-it", owners.Id);
        Assert.Equal("next-round", owners.EntryId);
        Assert.Equal("Brandon", owners.RuledBy);
        Assert.Equal(new DateOnly(2026, 9, 15), owners.RuledOn);
        Assert.Equal("docs/decisions/0007-brandons-rulings-on-six-open-questions-are-ruleset-version-seven.md", owners.Record);
        Assert.Contains("does another round begin", owners.Span, StringComparison.Ordinal);
    }

    [Fact]
    public void The_owners_ruling_is_named_only_where_both_sides_agreed_and_neither_was_defeated()
    {
        // With a side defeated the question does not arise, whatever was agreed: no round follows,
        // and the corpus's own sentence says so.
        var defeated = Value<NextRoundOutcome>(NextRound(
            2,
            ["Aria", "Goblin", "Bram"],
            SideDefeatedStatement.Defeated(Gm, "the goblins"),
            SidesAgreementStatement.Agreed(Gm)));

        Assert.False(defeated.Continues);
        Assert.Empty(defeated.Rulings);

        // Neither defeated and no agreement: the fight continues, on the corpus alone.
        var continues = Value<NextRoundOutcome>(NextRound(
            2,
            ["Aria", "Goblin", "Bram"],
            SideDefeatedStatement.Neither(Gm),
            SidesAgreementStatement.NotAgreed(Gm)));

        Assert.True(continues.Continues);
        Assert.Empty(continues.Rulings);

        // And before everyone has taken a turn, nothing of the question is reached.
        var early = Value<NextRoundOutcome>(EntryPoints.NextRound.Resolve(new NextRoundRequest
        {
            Round = 2,
            Order = ["Aria", "Goblin", "Bram"],
            TurnsTaken = ["Aria"],
            Defeat = SideDefeatedStatement.Neither(Gm),
            Agreement = SidesAgreementStatement.Agreed(Gm),
        }));

        Assert.False(early.RoundOver);
        Assert.Empty(early.Rulings);
    }
}
