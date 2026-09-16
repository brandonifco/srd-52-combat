using RulesKernel.Resolution;
using Srd52Combat.Requests;
using Srd52Combat.Turn;
using Xunit;
using static Srd52Combat.Tests.Fixtures;

namespace Srd52Combat.Tests;

/// <summary>
/// <c>free-object-interaction</c> and <c>communication-cost</c> ("Combat / Your Turn / p. 13") and
/// the gate on both, <c>gm-requires-action</c> (p. 14), through their entry points only.
/// </summary>
public class YourTurnEntryPointTests
{
    private const string Door = "the door";

    private const string Wand = "a wand of magic missiles";

    private const string Shout = "\"behind you!\"";

    private static RuleRequest Asserting(GmActionRequirement requirement) =>
        RuleRequest.Empty.Assert("gm-requires-action", requirement);

    private static Resolution<object> Interactions(GmActionRequirement requirement, params ObjectInteraction[] interactions) =>
        EntryPoints.FreeObjectInteraction.Resolve(new FreeObjectInteractionRequest(Asserting(requirement)) { Interactions = interactions });

    private static Resolution<object> Cost(GmActionRequirement requirement, CommunicationOnTurn communication) =>
        EntryPoints.CommunicationCost.Resolve(new CommunicationCostRequest(Asserting(requirement)) { Communication = communication });

    [Fact]
    public void The_first_interaction_is_free_during_the_move_or_during_the_action_and_a_second_needs_the_Utilize_action()
    {
        var duringMove = Value<TurnInteractions>(Interactions(
            GmActionRequirement.None(Gm),
            new ObjectInteraction(Door, InteractionTiming.DuringMove)));
        Assert.Equal(InteractionCostKind.Free, duringMove.Costs[0].Cost);
        Assert.Equal("Combat / Your Turn / p. 13", duringMove.Costs[0].Authority.Citation);
        Assert.Equal(EntryPoints.FreeObjectInteraction.Registered.Locator, duringMove.Authority);

        var duringAction = Value<TurnInteractions>(Interactions(
            GmActionRequirement.None(Gm),
            new ObjectInteraction(Door, InteractionTiming.DuringAction)));
        Assert.Equal(InteractionCostKind.Free, duringAction.Costs[0].Cost);

        var second = Value<TurnInteractions>(Interactions(
            GmActionRequirement.None(Gm),
            new ObjectInteraction(Door, InteractionTiming.DuringMove),
            new ObjectInteraction("a lever", InteractionTiming.DuringAction)));
        Assert.Equal(
            new[] { InteractionCostKind.Free, InteractionCostKind.RequiresUtilizeAction },
            second.Costs.Select(c => c.Cost));
        Assert.Equal(Door, second.Free!.Interaction.What);
    }

    [Fact]
    public void An_interaction_the_GM_requires_an_action_for_is_not_free_and_cites_page_14()
    {
        var requirement = GmActionRequirement.Of(Gm, RequiredActivity.Interaction("a stuck door"));
        var costs = Value<TurnInteractions>(Interactions(
            requirement,
            new ObjectInteraction("a stuck door", InteractionTiming.DuringMove),
            new ObjectInteraction(Door, InteractionTiming.DuringAction)));

        Assert.Equal(InteractionCostKind.RequiresAction, costs.Costs[0].Cost);
        Assert.Equal("Combat / Your Turn / p. 14", costs.Costs[0].Authority.Citation);
        Assert.Equal(EntryPoints.GmRequiresAction.Registered.Locator, costs.Costs[0].Authority);

        // The rule is suspended for that activity only: the next interaction is still the free one.
        Assert.Equal(InteractionCostKind.Free, costs.Costs[1].Cost);
        Assert.Same(requirement, costs.Requirement);
    }

    [Fact]
    public void An_object_whose_description_always_requires_an_action_is_not_the_free_interaction()
    {
        var costs = Value<TurnInteractions>(Interactions(
            GmActionRequirement.None(Gm),
            new ObjectInteraction(Wand, InteractionTiming.DuringAction, AlwaysRequiresAction: true),
            new ObjectInteraction(Door, InteractionTiming.DuringMove)));

        Assert.Equal(
            new[] { InteractionCostKind.RequiresAction, InteractionCostKind.Free },
            costs.Costs.Select(c => c.Cost));
        Assert.Contains("as stated in its description", costs.Costs[0].Why, StringComparison.Ordinal);
    }

    [Fact]
    public void The_GMs_requirement_is_demanded_never_inferred()
    {
        var interaction = Assert.Throws<AssertionRequiredException>(() =>
            EntryPoints.FreeObjectInteraction.Resolve(new FreeObjectInteractionRequest
            {
                Interactions = new[] { new ObjectInteraction(Door, InteractionTiming.DuringMove) },
            }));
        Assert.Equal("gm-requires-action", interaction.EntryId);

        var communication = Assert.Throws<AssertionRequiredException>(() =>
            EntryPoints.CommunicationCost.Resolve(new CommunicationCostRequest
            {
                Communication = new CommunicationOnTurn(CommunicationKind.Brief, Shout, Table),
            }));
        Assert.Equal("gm-requires-action", communication.EntryId);
    }

    [Fact]
    public void The_GMs_requirement_is_recorded_as_stated_and_the_engine_applies_no_measure_of_its_own()
    {
        var requirement = GmActionRequirement.Of(
            Gm,
            RequiredActivity.Interaction("a crank to lower a drawbridge"),
            RequiredActivity.Communication("a detailed explanation"));

        var answered = Value<GmActionRequirement>(
            EntryPoints.GmRequiresAction.Resolve(GmRequiresActionRequest.Asserting(requirement)));

        Assert.Same(requirement, answered);
        Assert.Equal(Gm, answered.StatedBy);
        Assert.True(answered.Requires(ActivityKind.ObjectInteraction, "a crank to lower a drawbridge"));
        Assert.False(answered.Requires(ActivityKind.ObjectInteraction, Door));

        var missing = Assert.Throws<AssertionRequiredException>(() =>
            EntryPoints.GmRequiresAction.Resolve(GmRequiresActionRequest.Empty));
        Assert.Equal("gm-requires-action", missing.EntryId);

        var wrongType = Assert.Throws<ArgumentException>(() =>
            EntryPoints.GmRequiresAction.Resolve(GmRequiresActionRequest.Asserting("the GM says so")));
        Assert.Contains("must be a GmActionRequirement", wrongType.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void The_parties_a_requirement_may_name_are_exactly_the_maps_assertedBy_for_gm_requires_action()
    {
        Assert.Equal(
            MapEntries.GmRequiresAction.AssertedBy,
            ActionRequirers.Allowed.Select(ActionRequirers.AssertedBy));
        Assert.Equal(new[] { "GM" }, EntryPoints.GmRequiresAction.Registered.AssertedBy);
        Assert.Equal(ActionRequirer.Gm, ActionRequirers.Named("GM"));

        var notNamed = Assert.Throws<InvalidOperationException>(() => ActionRequirers.Named("the players"));
        Assert.Contains("map assertedBy", notNamed.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Brief_communication_is_free_and_extended_communication_requires_an_action()
    {
        var brief = Value<CommunicationCost>(Cost(
            GmActionRequirement.None(Gm),
            new CommunicationOnTurn(CommunicationKind.Brief, Shout, Table)));
        Assert.Equal(CommunicationCostKind.Free, brief.Cost);
        Assert.Equal("Combat / Your Turn / p. 13", brief.Authority.Citation);
        Assert.Equal(EntryPoints.CommunicationCost.Registered.Locator, brief.Authority);

        var extended = Value<CommunicationCost>(Cost(
            GmActionRequirement.None(Gm),
            new CommunicationOnTurn(CommunicationKind.Extended, "an attempt to persuade a foe", Table)));
        Assert.Equal(CommunicationCostKind.RequiresAction, extended.Cost);
    }

    [Fact]
    public void A_GM_requirement_naming_the_communication_declines_citing_page_13()
    {
        var declined = Declined(Cost(
            GmActionRequirement.Of(Gm, RequiredActivity.Communication(Shout)),
            new CommunicationOnTurn(CommunicationKind.Brief, Shout, Table)));

        Assert.Equal(UnresolvedReason.RequiresInterpretation, declined.Reason);
        Assert.Equal(EntryPoints.CommunicationCost.Registered.Locator, declined.Locator);
        Assert.Contains("communication-cost", declined.Attempted, StringComparison.Ordinal);
        Assert.Contains("whether the GM's requirement reaches communication is not stated", declined.Attempted, StringComparison.Ordinal);
    }

    [Fact]
    public void A_GM_requirement_naming_only_an_object_interaction_leaves_communication_as_it_is()
    {
        var brief = Value<CommunicationCost>(Cost(
            GmActionRequirement.Of(Gm, RequiredActivity.Interaction("a stuck door")),
            new CommunicationOnTurn(CommunicationKind.Brief, Shout, Table)));

        Assert.Equal(CommunicationCostKind.Free, brief.Cost);

        // Nor does a requirement naming some other communication reach this one.
        var other = Value<CommunicationCost>(Cost(
            GmActionRequirement.Of(Gm, RequiredActivity.Communication("a detailed explanation")),
            new CommunicationOnTurn(CommunicationKind.Brief, Shout, Table)));
        Assert.Equal(CommunicationCostKind.Free, other.Cost);
    }
}
