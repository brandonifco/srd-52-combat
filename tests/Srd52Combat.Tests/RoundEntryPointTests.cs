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
    public void Both_sides_agreeing_to_end_with_neither_defeated_declines_citing_page_13()
    {
        var declined = Declined(NextRound(
            2,
            ["Aria", "Goblin", "Bram"],
            SideDefeatedStatement.Neither(Gm),
            SidesAgreementStatement.Agreed(Gm)));

        Assert.Equal(UnresolvedReason.RequiresInterpretation, declined.Reason);
        Assert.Equal(EntryPoints.NextRound.Registered.Locator, declined.Locator);
        Assert.Contains("next-round", declined.Attempted, StringComparison.Ordinal);
        Assert.Contains("combat-end", declined.Attempted, StringComparison.Ordinal);

        // With a side defeated as well, the question does not arise: no round follows.
        var defeated = Value<NextRoundOutcome>(NextRound(
            2,
            ["Aria", "Goblin", "Bram"],
            SideDefeatedStatement.Defeated(Gm, "the goblins"),
            SidesAgreementStatement.Agreed(Gm)));
        Assert.False(defeated.Continues);
    }
}
