using System.Collections.Immutable;
using RulesKernel.Provenance;
using Srd52Combat.Movement;

namespace Srd52Combat.Mounts;

/// <summary>
/// What <c>appropriate-anatomy</c> answers, "Combat / Mounted Combat / p. 15": whether this
/// creature has an anatomy appropriate to a mount. The corpus states no measure and names nobody who
/// decides; Brandon ruled on 2026-09-16 that it is the GM's call
/// (<c>appropriate-anatomy/gm-decides</c>, <c>docs/decisions/0007</c>), so the answer is the GM's
/// determination, reported with the ruling that makes it the answer. Where the GM has determined
/// nothing the rule declines instead, and a decline names no ruling.
/// </summary>
/// <param name="Appropriate">What the GM determined.</param>
/// <param name="Statement">The GM's determination, as the caller supplied it.</param>
/// <param name="Because">Why, in the engine's words.</param>
/// <param name="Authority">The rule: "Combat / Mounted Combat / p. 15".</param>
/// <param name="Rulings">The owner's ruling this answer relies on: <c>appropriate-anatomy/gm-decides</c>, always.</param>
public sealed record AnatomyRuling(
    bool Appropriate,
    AnatomyStatement Statement,
    string Because,
    SourceLocator Authority,
    ImmutableArray<OwnerRuling> Rulings)
{
    /// <summary>The rulings, checked to be present.</summary>
    public ImmutableArray<OwnerRuling> Rulings { get; } =
        Rulings.IsDefault ? throw new ArgumentNullException(nameof(Rulings)) : Rulings;

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Statement.Candidate} {(Appropriate ? "has" : "does not have")} an anatomy appropriate to a mount: {Because} [{Authority.Citation}]";
}

/// <summary>
/// What <c>mount-eligibility</c> says of a candidate: "A willing creature that is at least one
/// size larger than a rider and that has an appropriate anatomy can serve as a mount", "Combat /
/// Mounted Combat / p. 15". Two limbs the rule measures itself; the third is
/// <c>appropriate-anatomy</c>, which the GM decides under Brandon's ruling, so where willingness and
/// size hold the answer follows the GM's determination and names that ruling.
/// </summary>
/// <param name="CanServeAsAMount">Whether the creature can serve as a mount.</param>
/// <param name="Rider">The rider's size category.</param>
/// <param name="Candidate">The candidate's size category.</param>
/// <param name="Because">Which limb of the sentence decided it, in the engine's words.</param>
/// <param name="Authority">The rule: "Combat / Mounted Combat / p. 15".</param>
/// <param name="Rulings">
/// The owner's rulings this answer relies on: <c>appropriate-anatomy/gm-decides</c> where the
/// anatomy limb is what decided it, carried from <c>appropriate-anatomy</c>'s own answer, and none
/// where willingness or size decided it, which the rule measures for itself.
/// </param>
public sealed record MountEligibility(
    bool CanServeAsAMount,
    CreatureSize Rider,
    CreatureSize Candidate,
    string Because,
    SourceLocator Authority,
    ImmutableArray<OwnerRuling> Rulings)
{
    /// <summary>The rulings, checked to be present, even when there are none.</summary>
    public ImmutableArray<OwnerRuling> Rulings { get; } =
        Rulings.IsDefault ? throw new ArgumentNullException(nameof(Rulings)) : Rulings;

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
/// <param name="CanBeControlled">True when the corpus names the creature as having such training, or the caller states it.</param>
/// <param name="Creature">The creature, as the caller stated it.</param>
/// <param name="Because">Why, in the engine's words.</param>
/// <param name="Authority">The rule: "Combat / Controlling a Mount / p. 16".</param>
/// <param name="Rulings">
/// The owner's rulings this answer relies on: <c>mount-control-requires-training/training-is-stated</c>
/// where the caller stated the training of a creature the corpus does not name, and none for a
/// domesticated horse or a mule, whom the corpus itself names.
/// </param>
public sealed record MountControl(
    bool CanBeControlled,
    MountCreatureStatement Creature,
    string Because,
    SourceLocator Authority,
    ImmutableArray<OwnerRuling> Rulings)
{
    /// <summary>The rulings, checked to be present, even when there are none.</summary>
    public ImmutableArray<OwnerRuling> Rulings { get; } =
        Rulings.IsDefault ? throw new ArgumentNullException(nameof(Rulings)) : Rulings;

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
/// <param name="Rulings">
/// The owner's rulings this turn relies on: whatever <see cref="Control"/> relied on, carried through,
/// because a controlled mount's turn rests on its being trained (rules-factory decision 0027 § 4, as
/// amended on 2026-09-15). This rule has no ruling of its own.
/// </param>
public sealed record ControlledMountTurn(
    bool InitiativeMatchesTheRider,
    bool MovesOnYourTurnAsYouDirect,
    ImmutableArray<ControlledMountAction> ActionOptions,
    bool ActsOnTheTurnItIsMounted,
    MountControl Control,
    SourceLocator Authority,
    ImmutableArray<OwnerRuling> Rulings)
{
    /// <summary>The rulings, checked to be present, even when there are none.</summary>
    public ImmutableArray<OwnerRuling> Rulings { get; } =
        Rulings.IsDefault ? throw new ArgumentNullException(nameof(Rulings)) : Rulings;

    /// <summary>Whether an action is one of the three the rule allows.</summary>
    /// <param name="action">The action asked about.</param>
    /// <returns>True when the rule allows it.</returns>
    public bool Allows(ControlledMountAction action) => ActionOptions.Contains(action);

    /// <inheritdoc/>
    public override string ToString() =>
        MovesOnYourTurnAsYouDirect
            ? $"the mount's Initiative changes to match the rider's, it moves on the rider's turn as directed, its only action options are "
              + $"{string.Join(", ", ActionOptions)}, and it can move and act even on the turn it is mounted [{Authority.Citation}]"
            : $"the mount is not controlled, so this rule gives it nothing: {Control} [{Authority.Citation}]";
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
