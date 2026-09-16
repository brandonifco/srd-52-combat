using System.Collections.Immutable;
using RulesKernel.Provenance;
using Srd52Combat.Movement;

namespace Srd52Combat.Mounts;

/// <summary>
/// What <c>mount-eligibility</c> can say of a candidate: "A willing creature that is at least one
/// size larger than a rider and that has an appropriate anatomy can serve as a mount", "Combat /
/// Mounted Combat / p. 15". The rule answers only where a limb it can measure fails; where
/// willingness and size both hold, what remains is <c>appropriate-anatomy</c>, and the engine
/// declines rather than choosing a reading.
/// </summary>
/// <param name="CanServeAsAMount">False: the answer this rule gives is the refusal.</param>
/// <param name="Rider">The rider's size category.</param>
/// <param name="Candidate">The candidate's size category.</param>
/// <param name="Because">Which limb of the sentence fails, in the engine's words.</param>
/// <param name="Authority">The rule: "Combat / Mounted Combat / p. 15".</param>
public sealed record MountEligibility(
    bool CanServeAsAMount,
    CreatureSize Rider,
    CreatureSize Candidate,
    string Because,
    SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        $"a {Candidate} creature {(CanServeAsAMount ? "can" : "can't")} serve as a mount for a {Rider} rider: {Because} [{Authority.Citation}]";
}

/// <summary>
/// What mounting or dismounting costs: <c>mounting-cost</c>, "Combat / Mounting and Dismounting /
/// p. 15". "Doing so costs an amount of movement equal to half your Speed (round down)."
/// </summary>
/// <param name="Action">Mounting or dismounting.</param>
/// <param name="CostFeet">The cost in feet: half the Speed, rounded down.</param>
/// <param name="Done">True when the cost was paid and the rider mounted or dismounted.</param>
/// <param name="Deduction">What <c>movement-deduction</c> made of the cost against the movement left.</param>
/// <param name="Because">Why, in the engine's words.</param>
/// <param name="Authority">The rule: "Combat / Mounting and Dismounting / p. 15".</param>
public sealed record MountingMove(
    MountAction Action,
    int CostFeet,
    bool Done,
    MovementSpent Deduction,
    string Because,
    SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        Done
            ? $"{Action} costs {CostFeet} ft of movement, and it is paid: {Because} [{Authority.Citation}]"
            : $"{Action} costs {CostFeet} ft of movement, and it is not paid: {Because} [{Authority.Citation}]";
}

/// <summary>
/// Whether a mount can be controlled: <c>mount-control-requires-training</c>, "Combat / Controlling
/// a Mount / p. 16". "You can control a mount only if it has been trained to accept a rider."
/// </summary>
/// <param name="CanBeControlled">True when the corpus names the creature as having such training.</param>
/// <param name="Creature">The creature, as the caller stated it.</param>
/// <param name="Because">Why, in the engine's words.</param>
/// <param name="Authority">The rule: "Combat / Controlling a Mount / p. 16".</param>
public sealed record MountControl(
    bool CanBeControlled,
    MountCreatureStatement Creature,
    string Because,
    SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        $"the mount {(CanBeControlled ? "can" : "can't")} be controlled: {Because} [{Authority.Citation}]";
}

/// <summary>
/// A controlled mount's turn: <c>controlled-mount-turn</c>, "Combat / Controlling a Mount / p. 16".
/// </summary>
/// <param name="InitiativeMatchesTheRider">"The Initiative of a controlled mount changes to match yours when you mount it."</param>
/// <param name="MovesOnYourTurnAsYouDirect">"It moves on your turn as you direct it."</param>
/// <param name="ActionOptions">The three action options, a closed set: Dash, Disengage and Dodge.</param>
/// <param name="ActsOnTheTurnItIsMounted">"A controlled mount can move and act even on the turn that you mount it."</param>
/// <param name="Control">What <c>mount-control-requires-training</c> said of the mount.</param>
/// <param name="Authority">The rule: "Combat / Controlling a Mount / p. 16".</param>
public sealed record ControlledMountTurn(
    bool InitiativeMatchesTheRider,
    bool MovesOnYourTurnAsYouDirect,
    ImmutableArray<ControlledMountAction> ActionOptions,
    bool ActsOnTheTurnItIsMounted,
    MountControl Control,
    SourceLocator Authority)
{
    /// <summary>Whether an action is one of the three the rule allows.</summary>
    /// <param name="action">The action asked about.</param>
    /// <returns>True when the rule allows it.</returns>
    public bool Allows(ControlledMountAction action) => ActionOptions.Contains(action);

    /// <inheritdoc/>
    public override string ToString() =>
        $"the mount's Initiative changes to match the rider's, it moves on the rider's turn as directed, its only action options are "
        + $"{string.Join(", ", ActionOptions)}, and it can move and act even on the turn it is mounted [{Authority.Citation}]";
}

/// <summary>
/// An independent mount: <c>independent-mount</c>, "Combat / Controlling a Mount / p. 16". "In
/// contrast, an independent mount—one that lets you ride but ignores your control—retains its place
/// in the Initiative order and moves and acts as it likes."
/// </summary>
/// <param name="Independent">True when the caller states the mount ignores the rider's control.</param>
/// <param name="RetainsItsPlaceInTheInitiativeOrder">True for an independent mount; a controlled one is <c>controlled-mount-turn</c>'s.</param>
/// <param name="MovesAndActsAsItLikes">True for an independent mount; which moves and actions those are the corpus does not decide.</param>
/// <param name="Behaviour">What the caller stated about the rider's control.</param>
/// <param name="Because">Why, in the engine's words.</param>
/// <param name="Authority">The rule: "Combat / Controlling a Mount / p. 16".</param>
public sealed record IndependentMount(
    bool Independent,
    bool RetainsItsPlaceInTheInitiativeOrder,
    bool MovesAndActsAsItLikes,
    MountBehaviourStatement Behaviour,
    string Because,
    SourceLocator Authority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        Independent
            ? $"the mount is independent: it retains its place in the Initiative order and moves and acts as it likes; {Because} [{Authority.Citation}]"
            : $"the mount is not independent: {Because} [{Authority.Citation}]";
}

/// <summary>
/// What "Combat / Falling Off / p. 16" demands of a rider, and what the stated save did with it:
/// <c>falling-off</c>.
/// </summary>
/// <param name="Trigger">Which of the rule's three cases is in play.</param>
/// <param name="SavingThrowDc">The DC the rule prints: 10.</param>
/// <param name="Ability">The ability the rule names: Dexterity.</param>
/// <param name="Outcome">What the save did, as the caller stated it; null when the caller only asked what the rule demands.</param>
/// <param name="StaysOn">True when the save succeeded; null when no save outcome was stated.</param>
/// <param name="Because">Why, in the engine's words.</param>
/// <param name="Authority">The rule: "Combat / Falling Off / p. 16".</param>
/// <param name="SaveAuthority">Where the saving throw itself is made: <c>saving-throws</c>, outside this engine's extent.</param>
public sealed record FallingOffRuling(
    FallTrigger Trigger,
    int SavingThrowDc,
    string Ability,
    SaveOutcome? Outcome,
    bool? StaysOn,
    string Because,
    SourceLocator Authority,
    SourceLocator SaveAuthority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        StaysOn is null
            ? $"a DC {SavingThrowDc} {Ability} saving throw is required [{Authority.Citation}]: {Because}; the save is made under [{SaveAuthority.Citation}]"
            : $"a DC {SavingThrowDc} {Ability} saving throw is required and the rider stays on [{Authority.Citation}]: {Because}";
}
