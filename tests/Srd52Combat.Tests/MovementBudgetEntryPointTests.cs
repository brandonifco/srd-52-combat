using Srd52Combat.Movement;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// "Combat / Movement and Position / p. 14", through the entry points only: how far a creature may
/// move (<c>move-up-to-speed</c>) and how the parts of a move are deducted from its Speed
/// (<c>movement-deduction</c>). These are the budget <c>mounting-cost</c> spends.
/// </summary>
public class MovementBudgetEntryPointTests
{
    [Fact]
    public void A_distance_equal_to_the_Speed_or_less_may_be_moved_and_more_may_not()
    {
        (int Distance, bool Allowed)[] cases = [(0, true), (15, true), (30, true), (31, false)];

        foreach (var (distance, allowed) in cases)
        {
            var ruling = Value<MoveAllowance>(EntryPoints.MoveUpToSpeed.Resolve(new MoveUpToSpeedRequest
            {
                SpeedInFeet = 30,
                DistanceFeet = distance,
                StatedBy = Gm,
            }));

            Assert.Equal(allowed, ruling.Allowed);
            Assert.Equal(30, ruling.SpeedInFeet);
            Assert.Equal("Combat / Movement and Position / p. 14", ruling.Authority.Citation);
            Assert.Equal(EntryPoints.MoveUpToSpeed.Registered.Locator, ruling.Authority);
        }
    }

    [Fact]
    public void Each_part_of_a_move_is_deducted_from_the_Speed_in_the_order_stated()
    {
        var spent = Value<MovementSpent>(EntryPoints.MovementDeduction.Resolve(new MovementDeductionRequest
        {
            SpeedInFeet = 30,
            StatedBy = Gm,
            Parts = [new MovePart("walking to the door", 10), new MovePart("climbing the wall", 15)],
        }));

        Assert.Equal(["walking to the door", "climbing the wall"], spent.Taken.Select(p => p.Description));
        Assert.Empty(spent.NotTaken);
        Assert.Equal(5, spent.MovementLeftFeet);
        Assert.False(spent.SpeedUsedUp);
        Assert.Equal("Combat / Movement and Position / p. 14", spent.Authority.Citation);
    }

    [Fact]
    public void A_part_the_movement_left_does_not_cover_is_not_taken_and_neither_is_what_follows()
    {
        var spent = Value<MovementSpent>(EntryPoints.MovementDeduction.Resolve(new MovementDeductionRequest
        {
            SpeedInFeet = 30,
            StatedBy = Gm,
            Parts =
            [
                new MovePart("walking to the door", 25),
                new MovePart("climbing the wall", 10),
                new MovePart("walking on", 5),
            ],
        }));

        Assert.Equal(["walking to the door"], spent.Taken.Select(p => p.Description));
        Assert.Equal(["climbing the wall", "walking on"], spent.NotTaken.Select(p => p.Description));
        Assert.Equal(5, spent.MovementLeftFeet);
        Assert.True(spent.SpeedUsedUp);
    }

    [Fact]
    public void A_figure_a_movement_rule_needs_is_refused_when_left_out_never_inferred()
    {
        Assert.Throws<ArgumentException>(() => EntryPoints.MoveUpToSpeed.Resolve(
            new MoveUpToSpeedRequest { SpeedInFeet = 30, StatedBy = Gm }));
        Assert.Throws<ArgumentException>(() => EntryPoints.MovementDeduction.Resolve(
            new MovementDeductionRequest { SpeedInFeet = 30, StatedBy = Gm }));
        Assert.Throws<ArgumentException>(() => EntryPoints.MoveUpToSpeed.Resolve(
            new MoveUpToSpeedRequest { SpeedInFeet = 30, DistanceFeet = 10 }));
    }
}
