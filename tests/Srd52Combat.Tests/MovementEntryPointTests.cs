using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Requests;
using Xunit;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// The size categories (<c>size-categories</c>, "Combat / Creature Size / p. 14"), the modes a move
/// can be made of (<c>movement-modes</c>, "Combat / Movement and Position / p. 14") and dropping
/// Prone (<c>dropping-prone</c>, "Combat / Dropping Prone / p. 14"), through their entry points only.
/// </summary>
public class MovementEntryPointTests
{
    [Fact]
    public void The_size_categories_run_from_Tiny_to_Gargantuan_citing_page_14()
    {
        var order = Value<SizeOrder>(EntryPoints.SizeCategories.Resolve(SizeCategoriesRequest.Empty));

        Assert.Equal(
            new[] { CreatureSize.Tiny, CreatureSize.Small, CreatureSize.Medium, CreatureSize.Large, CreatureSize.Huge, CreatureSize.Gargantuan },
            order.FromSmallestToLargest.AsEnumerable());
        Assert.Equal(2, order.Steps(CreatureSize.Medium, CreatureSize.Huge));
        Assert.Equal(-2, order.Steps(CreatureSize.Huge, CreatureSize.Medium));
        Assert.True(order.IsLarger(CreatureSize.Gargantuan, CreatureSize.Huge));
        Assert.False(order.IsLarger(CreatureSize.Medium, CreatureSize.Medium));
        Assert.Equal("Combat / Creature Size / p. 14", order.Authority.Citation);
        Assert.Equal(EntryPoints.SizeCategories.Registered.Locator, order.Authority);
    }

    [Fact]
    public void A_move_can_mix_modes_or_be_entirely_one_and_all_of_them_draw_on_the_same_Speed_citing_page_14()
    {
        var mixed = Value<CombinedMove>(Move(MovementMode.Regular, MovementMode.Climbing, MovementMode.Swimming));
        var entirely = Value<CombinedMove>(Move(MovementMode.Crawling));

        Assert.Equal(new[] { MovementMode.Regular, MovementMode.Climbing, MovementMode.Swimming }, mixed.Modes.AsEnumerable());
        Assert.True(mixed.CombinedWithRegularMovement);
        Assert.False(mixed.EntireMoveInOneMode);
        Assert.True(mixed.DrawsOnOneSpeed);
        Assert.True(entirely.EntireMoveInOneMode);
        Assert.True(entirely.DrawsOnOneSpeed);
        Assert.False(entirely.CombinedWithRegularMovement);
        Assert.Equal(Gm, mixed.StatedBy);
        Assert.Equal("Combat / Movement and Position / p. 14", mixed.Authority.Citation);
        Assert.Equal(EntryPoints.MovementModes.Registered.Locator, mixed.Authority);
    }

    [Fact]
    public void What_a_mode_costs_declines_citing_the_Rules_Glossary()
    {
        var declined = Declined(EntryPoints.MovementModes.Resolve(new MovementModesRequest
        {
            Modes = [MovementMode.Climbing],
            StatedBy = Gm,
            AskingWhatAModeCosts = MovementMode.Climbing,
        }));

        Assert.Equal(UnresolvedReason.OutsideCurrentScope, declined.Reason);
        Assert.Equal(EntryPoints.MovementModesGlossary.Registered.Locator, declined.Locator);
        Assert.Contains("'movement-modes'", declined.Attempted, StringComparison.Ordinal);
        Assert.Contains("Climbing", declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void A_creature_can_drop_Prone_for_free_unless_its_Speed_is_zero_citing_page_14()
    {
        var walking = Value<DroppingProneRuling>(Drop(30));
        var rooted = Value<DroppingProneRuling>(Drop(0));

        Assert.True(walking.Allowed);
        Assert.False(walking.UsesAnAction);
        Assert.Equal(0, walking.SpeedSpent);
        Assert.False(rooted.Allowed);
        Assert.Equal("Combat / Dropping Prone / p. 14", walking.Authority.Citation);
        Assert.Equal(EntryPoints.DroppingProne.Registered.Locator, walking.Authority);
    }

    [Fact]
    public void A_movement_input_left_out_is_refused_never_inferred()
    {
        var noModes = Assert.Throws<ArgumentException>(() =>
            EntryPoints.MovementModes.Resolve(new MovementModesRequest { StatedBy = Gm }));
        Assert.Equal("Modes", noModes.ParamName);

        var noStatedBy = Assert.Throws<ArgumentException>(() =>
            EntryPoints.MovementModes.Resolve(new MovementModesRequest { Modes = [MovementMode.Regular] }));
        Assert.Equal("StatedBy", noStatedBy.ParamName);

        var noSpeed = Assert.Throws<ArgumentException>(() =>
            EntryPoints.DroppingProne.Resolve(new DroppingProneRequest()));
        Assert.Equal("SpeedInFeet", noSpeed.ParamName);

        Assert.Throws<ArgumentException>(() => EntryPoints.MovementModes.Resolve(
            new MovementModesRequest { Modes = [default], StatedBy = Gm }));
    }

    private static Resolution<object> Move(params MovementMode[] modes) =>
        EntryPoints.MovementModes.Resolve(new MovementModesRequest { Modes = modes, StatedBy = Gm });

    private static Resolution<object> Drop(int speedInFeet) =>
        EntryPoints.DroppingProne.Resolve(new DroppingProneRequest { SpeedInFeet = speedInFeet });
}
