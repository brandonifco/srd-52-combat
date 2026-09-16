using RulesKernel.Resolution;
using Srd52Combat.Requests;
using Srd52Combat.Turn;
using Xunit;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// <c>turn-move-and-action</c> and <c>doing-nothing</c> ("Combat / Your Turn", pp. 13 and 14) and
/// <c>break-up-move</c> ("Combat / Breaking Up Your Move / p. 14"), through their entry points only.
/// </summary>
public class TurnEntryPointTests
{
    private const string Attack = "the Attack action";

    private static Resolution<object> Turn(int speed, params TurnStep[] steps) =>
        EntryPoints.TurnMoveAndAction.Resolve(new TurnMoveAndActionRequest { Speed = speed, Steps = steps, StatedBy = Table });

    private static Resolution<object> Broken(int speed, params TurnStep[] steps) =>
        EntryPoints.BreakUpMove.Resolve(new BreakUpMoveRequest { Speed = speed, Steps = steps, StatedBy = Table });

    private static Resolution<object> Forgone(int speed, params TurnStep[] steps) =>
        EntryPoints.DoingNothing.Resolve(new DoingNothingRequest { Speed = speed, Steps = steps, StatedBy = Table });

    [Fact]
    public void On_your_turn_you_move_up_to_your_Speed_and_take_one_action_in_either_order()
    {
        var moveFirst = Value<TakenTurn>(Turn(30, TurnStep.Moves(30), TurnStep.Acts(Attack)));
        var actFirst = Value<TakenTurn>(Turn(30, TurnStep.Acts(Attack), TurnStep.Moves(30)));

        foreach (var turn in new[] { moveFirst, actFirst })
        {
            Assert.True(turn.WithinTurn);
            Assert.Equal(30, turn.MovementUsed);
            Assert.Equal(0, turn.MovementRemaining);
            Assert.True(turn.ActionTaken);
            Assert.Equal(1, turn.Actions);
            Assert.Equal("Combat / Your Turn / p. 13", turn.Authority.Citation);
            Assert.Equal(EntryPoints.TurnMoveAndAction.Registered.Locator, turn.Authority);
        }

        // The creature decides which comes first, and the budget is the same either way.
        Assert.Equal(TurnStepKind.Move, moveFirst.Started);
        Assert.Equal(TurnStepKind.Action, actFirst.Started);

        // Less than the Speed is a move up to it, and what is left is left.
        var upTo = Value<TakenTurn>(Turn(30, TurnStep.Moves(10)));
        Assert.True(upTo.WithinTurn);
        Assert.Equal(20, upTo.MovementRemaining);
        Assert.False(upTo.ActionTaken);
    }

    [Fact]
    public void A_turn_beyond_the_Speed_or_with_a_second_action_is_named_as_beyond_what_the_turn_allows()
    {
        var far = Value<TakenTurn>(Turn(30, TurnStep.Moves(35)));
        Assert.False(far.WithinTurn);
        Assert.Contains(far.Exceeded, e => e.Contains("35 feet", StringComparison.Ordinal) && e.Contains("Speed of 30", StringComparison.Ordinal));

        var twice = Value<TakenTurn>(Turn(30, TurnStep.Acts(Attack), TurnStep.Acts("the Dash action")));
        Assert.False(twice.WithinTurn);
        Assert.Equal(2, twice.Actions);
        Assert.Contains(twice.Exceeded, e => e.Contains("the turn allows one", StringComparison.Ordinal));

        // A Bonus Action and a Reaction are neither the move nor the action (pp. 10, outside the slice).
        var bonus = Value<TakenTurn>(Turn(30, TurnStep.Acts(Attack), TurnStep.BonusAction("a Bonus Action"), TurnStep.Reaction("an Opportunity Attack")));
        Assert.True(bonus.WithinTurn);
        Assert.Equal(1, bonus.Actions);
        Assert.Equal(0, bonus.MovementUsed);
    }

    [Fact]
    public void A_turn_stated_without_its_Speed_or_its_steps_is_refused_never_inferred()
    {
        var speed = Assert.Throws<ArgumentException>(() =>
            EntryPoints.TurnMoveAndAction.Resolve(new TurnMoveAndActionRequest { Steps = [], StatedBy = Table }));
        Assert.Equal("Speed", speed.ParamName);

        var steps = Assert.Throws<ArgumentException>(() =>
            EntryPoints.TurnMoveAndAction.Resolve(new TurnMoveAndActionRequest { Speed = 30, StatedBy = Table }));
        Assert.Equal("Steps", steps.ParamName);

        var statedBy = Assert.Throws<ArgumentException>(() =>
            EntryPoints.TurnMoveAndAction.Resolve(new TurnMoveAndActionRequest { Speed = 30, Steps = [] }));
        Assert.Equal("StatedBy", statedBy.ParamName);
    }

    [Fact]
    public void A_move_is_broken_up_around_an_action_and_what_is_left_is_the_remainder_not_a_fresh_Speed()
    {
        // The corpus's own example: Speed 30, 10 feet, an action, then 20 feet.
        var move = Value<BrokenMove>(Broken(30, TurnStep.Moves(10), TurnStep.Acts(Attack), TurnStep.Moves(20)));

        Assert.Collection(
            move.Segments,
            first =>
            {
                Assert.Equal(10, first.Feet);
                Assert.Null(first.After);
                Assert.Equal(20, first.RemainingAfter);
            },
            second =>
            {
                Assert.Equal(20, second.Feet);
                Assert.Equal(Attack, second.After);
                Assert.Equal(0, second.RemainingAfter);
            });
        Assert.True(move.WithinTurn);
        Assert.Equal(30, move.MovementUsed);
        Assert.Equal("Combat / Breaking Up Your Move / p. 14", move.Authority.Citation);
        Assert.Equal(EntryPoints.BreakUpMove.Registered.Locator, move.Authority);

        // The movement after the action is what remains: 10 feet then 30 more is past the Speed.
        var fresh = Value<BrokenMove>(Broken(30, TurnStep.Moves(10), TurnStep.Acts(Attack), TurnStep.Moves(30)));
        Assert.False(fresh.WithinTurn);
        Assert.Equal(40, fresh.MovementUsed);
        Assert.Equal(0, fresh.Segments[1].RemainingAfter);
    }

    [Fact]
    public void A_move_broken_around_a_Bonus_Action_and_a_Reaction_costs_neither_the_move_nor_the_action()
    {
        var move = Value<BrokenMove>(Broken(
            30,
            TurnStep.Moves(5),
            TurnStep.BonusAction("a Bonus Action"),
            TurnStep.Moves(5),
            TurnStep.Reaction("a Reaction"),
            TurnStep.Moves(5)));

        Assert.Equal(new[] { 5, 5, 5 }, move.Segments.Select(s => s.Feet));
        Assert.Equal(new string?[] { null, "a Bonus Action", "a Reaction" }, move.Segments.Select(s => s.After));
        Assert.Equal(new[] { 25, 20, 15 }, move.Segments.Select(s => s.RemainingAfter));
        Assert.True(move.WithinTurn);
        Assert.False(move.Turn.ActionTaken);
    }

    [Fact]
    public void A_turn_can_forgo_moving_acting_or_doing_anything_at_all()
    {
        var nothing = Value<ForgoneTurn>(Forgone(30));

        Assert.True(nothing.Everything);
        Assert.True(nothing.Movement);
        Assert.True(nothing.Action);
        Assert.True(nothing.Permitted);
        Assert.Equal(30, nothing.Turn.MovementRemaining);
        Assert.True(nothing.Turn.WithinTurn);
        Assert.Null(nothing.Turn.Started);
        Assert.Equal("Combat / Your Turn / p. 14", nothing.Authority.Citation);
        Assert.Equal(EntryPoints.DoingNothing.Registered.Locator, nothing.Authority);
    }

    [Fact]
    public void Forgoing_only_the_move_or_only_the_action_is_permitted_and_leaves_the_rest_of_the_turn()
    {
        var moveOnly = Value<ForgoneTurn>(Forgone(30, TurnStep.Moves(30)));
        Assert.False(moveOnly.Everything);
        Assert.False(moveOnly.Movement);
        Assert.True(moveOnly.Action);
        Assert.True(moveOnly.Permitted);

        var actionOnly = Value<ForgoneTurn>(Forgone(30, TurnStep.Acts(Attack)));
        Assert.True(actionOnly.Movement);
        Assert.False(actionOnly.Action);
        Assert.True(actionOnly.Permitted);
        Assert.Equal(30, actionOnly.Turn.MovementRemaining);

        var both = Value<ForgoneTurn>(Forgone(30, TurnStep.Moves(15), TurnStep.Acts(Attack)));
        Assert.False(both.Movement);
        Assert.False(both.Action);
        Assert.True(both.Permitted);
    }
}
