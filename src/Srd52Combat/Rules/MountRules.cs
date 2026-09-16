using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Mounts;
using Srd52Combat.Movement;

namespace Srd52Combat.Rules;

/// <summary>
/// Mounted combat, "Combat / Mounted Combat / p. 15" through "Combat / Falling Off / p. 16": who can
/// be a mount (<c>mount-eligibility</c>, <c>appropriate-anatomy</c>), what mounting costs
/// (<c>mounting-cost</c>), and what riding one is (<c>mount-control-requires-training</c>,
/// <c>controlled-mount-turn</c>, <c>independent-mount</c>, <c>falling-off</c>).
/// </summary>
/// <remarks>
/// <para>
/// Four of these entries cite "Combat / Controlling a Mount / p. 16" or "Combat / Falling Off /
/// p. 16", so every decline names its entry in <see cref="UnresolvedResult.Attempted"/>.
/// </para>
/// <para>
/// Three of the seven carry an unresolved question, and each declines
/// <see cref="UnresolvedReason.RequiresInterpretation"/> exactly where its answer turns on it:
/// what anatomy is appropriate for a mount (<c>appropriate-anatomy</c>, and so
/// <c>mount-eligibility</c> where willingness and size both hold), which creatures beyond the
/// corpus's own instances have been trained to accept a rider
/// (<c>mount-control-requires-training</c>), what makes a mount independent
/// (<c>independent-mount</c>), and which unoccupied space a rider who falls lands in
/// (<c>falling-off</c>).
/// </para>
/// <para>
/// Because <c>appropriate-anatomy</c> can never resolve, no chain of rules in this engine reaches a
/// state in which a creature is a mount. So the <c>enabledBy</c> gate the later entries name is the
/// caller's <see cref="MountStatement"/>: who is on what, stated and attributed, exactly as
/// <c>grid-play</c> is stated for the grid rules (decision 0005). Where nothing states it, the rule
/// declines <see cref="UnresolvedReason.OutsideCurrentScope"/> citing the gate that is not open.
/// </para>
/// </remarks>
public static class MountRules
{
    /// <summary>The DC the falling-off rule prints.</summary>
    public const int FallingOffDc = 10;

    /// <summary>The ability the falling-off rule names.</summary>
    public const string FallingOffAbility = "Dexterity";

    /// <summary>How near a creature must be to be mounted, in feet: "within 5 feet of you".</summary>
    public const int MountingReachFeet = 5;

    /// <summary>
    /// <c>appropriate-anatomy</c>: the gap split out of <c>mount-eligibility</c>. "An appropriate
    /// anatomy" states no measure and no set of values, and names nobody who decides, so every
    /// question this entry is asked turns on the question the corpus leaves open.
    /// </summary>
    /// <param name="candidate">The creature asked about.</param>
    /// <returns>The decline citing this entry.</returns>
    /// <exception cref="ArgumentException"><paramref name="candidate"/> is empty.</exception>
    public static Resolution<object> AppropriateAnatomy(string candidate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(candidate);
        return Declines.Of<object>(
            UnresolvedReason.RequiresInterpretation,
            $"{Declines.Attempting(MapEntries.AppropriateAnatomy)} for {candidate}: \"an appropriate anatomy\" states no measure and "
            + "no set of values, and names nobody who decides",
            MapEntries.AppropriateAnatomy);
    }

    /// <summary>
    /// <c>mount-eligibility</c>: "A willing creature that is at least one size larger than a rider
    /// and that has an appropriate anatomy can serve as a mount, using the following rules."
    /// </summary>
    /// <remarks>
    /// The sentence is three conditions together. Two of them the engine can measure: willingness is
    /// a fact the caller states, and "at least one size larger" is counted along
    /// <c>size-categories</c>. The third is <c>appropriate-anatomy</c>, which resolves nothing, so
    /// the rule answers only the refusals: where willingness or size fails, no anatomy makes the
    /// creature a mount. Where both hold, what remains is the anatomy, and the rule declines.
    /// </remarks>
    /// <param name="rider">The rider's size category, as the caller states it.</param>
    /// <param name="candidate">The candidate's name or handle.</param>
    /// <param name="candidateSize">The candidate's size category, as the caller states it.</param>
    /// <param name="willing">Whether the candidate is willing, as the caller states it.</param>
    /// <returns>That the creature can't serve as a mount, or the decline citing <c>appropriate-anatomy</c>.</returns>
    public static Resolution<MountEligibility> Eligible(
        CreatureSize rider,
        string candidate,
        CreatureSize candidateSize,
        WillingStatement willing)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(candidate);
        ArgumentNullException.ThrowIfNull(willing);
        Checks.Defined(rider, nameof(rider));
        Checks.Defined(candidateSize, nameof(candidateSize));

        var order = MovementRules.Sizes();
        int steps = order.Steps(rider, candidateSize);
        if (!willing.Willing)
        {
            return Resolution<MountEligibility>.FromValue(new MountEligibility(
                CanServeAsAMount: false,
                rider,
                candidateSize,
                $"the rule names a willing creature, and {willing}",
                MapEntries.MountEligibility.Locator));
        }

        if (steps < 1)
        {
            return Resolution<MountEligibility>.FromValue(new MountEligibility(
                CanServeAsAMount: false,
                rider,
                candidateSize,
                $"the rule names a creature at least one size larger than the rider, and {candidateSize} is "
                + $"{(steps == 0 ? "the rider's own size" : $"{-steps} size(s) smaller than the rider")} along {order}",
                MapEntries.MountEligibility.Locator));
        }

        return Declines.Of<MountEligibility>(
            UnresolvedReason.RequiresInterpretation,
            $"{Declines.Attempting(MapEntries.MountEligibility)} for {candidate}: it is willing and {steps} size(s) larger than the "
            + $"rider, so what is left of the sentence is \"an appropriate anatomy\", which is '{MapEntries.AppropriateAnatomy.Id}', "
            + "and the corpus states no measure for it",
            MapEntries.AppropriateAnatomy);
    }

    /// <summary>
    /// <c>mounting-cost</c>: "During your move, you can mount a creature that is within 5 feet of
    /// you or dismount. Doing so costs an amount of movement equal to half your Speed (round down).
    /// For example, if your Speed is 30 feet, you spend 15 feet of movement to mount a horse."
    /// </summary>
    /// <remarks>
    /// The cost is taken out of the movement left through <c>movement-deduction</c>, which deducts
    /// "the distance of each part of your move from it until it is used up". A creature with less
    /// movement left than the cost does not mount: no rule of the slice takes the total past the
    /// Speed (<c>move-up-to-speed</c>). The 5 feet qualify the creature you mount; the corpus
    /// attaches no distance to dismounting.
    /// </remarks>
    /// <param name="action">Mounting or dismounting.</param>
    /// <param name="speedInFeet">The rider's Speed in feet, as the caller states it.</param>
    /// <param name="movementLeftFeet">The movement left in this move, in feet, as the caller states it.</param>
    /// <param name="distanceToMountFeet">How far away the creature is, in feet; read only when mounting.</param>
    /// <param name="mount">Whether the creature serves as a mount, as the caller states it.</param>
    /// <returns>What it costs and whether it is done, or the decline citing <c>mount-eligibility</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A figure is negative, or the action is not stated.</exception>
    public static Resolution<MountingMove> Mounting(
        MountAction action,
        int speedInFeet,
        int movementLeftFeet,
        int distanceToMountFeet,
        MountStatement mount)
    {
        ArgumentNullException.ThrowIfNull(mount);
        ArgumentOutOfRangeException.ThrowIfNegative(speedInFeet);
        ArgumentOutOfRangeException.ThrowIfNegative(movementLeftFeet);
        ArgumentOutOfRangeException.ThrowIfNegative(distanceToMountFeet);
        if (!Enum.IsDefined(action))
        {
            throw new ArgumentOutOfRangeException(nameof(action), action, "the rule prices mounting and dismounting");
        }

        if (NotAMount(MapEntries.MountingCost, mount) is { } declined)
        {
            return Resolution<MountingMove>.FromUnresolved(declined);
        }

        int cost = speedInFeet / 2;
        var deduction = MovementBudgetRules.Deduct(
            movementLeftFeet,
            [new MovePart($"{action.ToString().ToLowerInvariant()}ing {mount.Mount}", cost)],
            mount.StatedBy);

        if (action == MountAction.Mount && distanceToMountFeet > MountingReachFeet)
        {
            return Resolution<MountingMove>.FromValue(new MountingMove(
                action,
                cost,
                Done: false,
                deduction,
                $"the rule lets you mount a creature that is within {MountingReachFeet} feet of you, and {mount.Mount} is "
                + $"{distanceToMountFeet} feet away",
                MapEntries.MountingCost.Locator));
        }

        return Resolution<MountingMove>.FromValue(new MountingMove(
            action,
            cost,
            Done: !deduction.SpeedUsedUp,
            deduction,
            deduction.SpeedUsedUp
                ? $"half a Speed of {speedInFeet} ft rounded down is {cost} ft, and {movementLeftFeet} ft of movement is left, which does not cover it"
                : $"half a Speed of {speedInFeet} ft rounded down is {cost} ft, deducted from the {movementLeftFeet} ft left, leaving {deduction.MovementLeftFeet} ft",
            MapEntries.MountingCost.Locator));
    }

    /// <summary>
    /// <c>mount-control-requires-training</c>: "You can control a mount only if it has been trained
    /// to accept a rider. Domesticated horses, mules, and similar creatures have such training."
    /// </summary>
    /// <remarks>
    /// The corpus gives instances and no definition. For a domesticated horse or a mule it says the
    /// training is there, and the rule answers. For any other creature "similar" states no measure
    /// and nobody is named who decides, so the rule declines
    /// <see cref="UnresolvedReason.RequiresInterpretation"/> rather than reading "similar" for the
    /// corpus.
    /// </remarks>
    /// <param name="creature">Which creature the mount is, as the caller states it.</param>
    /// <param name="mount">Whether a rider is on it, as the caller states it.</param>
    /// <returns>Whether it can be controlled, or a decline.</returns>
    public static Resolution<MountControl> Control(MountCreatureStatement creature, MountStatement mount)
    {
        ArgumentNullException.ThrowIfNull(creature);
        ArgumentNullException.ThrowIfNull(mount);
        if (NotRidden(MapEntries.MountControlRequiresTraining, mount) is { } declined)
        {
            return Resolution<MountControl>.FromUnresolved(declined);
        }

        return creature.NamedByTheCorpus
            ? Resolution<MountControl>.FromValue(new MountControl(
                CanBeControlled: true,
                creature,
                $"the corpus names domesticated horses and mules as having the training a rider needs, and {creature}",
                MapEntries.MountControlRequiresTraining.Locator))
            : Declines.Of<MountControl>(
                UnresolvedReason.RequiresInterpretation,
                $"{Declines.Attempting(MapEntries.MountControlRequiresTraining)} for {creature}: the corpus names domesticated "
                + "horses, mules \"and similar creatures\", which states no measure, and nothing in the slice says whether this "
                + "creature is trained or who decides",
                MapEntries.MountControlRequiresTraining);
    }

    /// <summary>
    /// <c>controlled-mount-turn</c>: "The Initiative of a controlled mount changes to match yours
    /// when you mount it. It moves on your turn as you direct it, and it has only three action
    /// options during that turn: Dash, Disengage, and Dodge. A controlled mount can move and act
    /// even on the turn that you mount it."
    /// </summary>
    /// <remarks>
    /// A mount is controlled only if it is trained (<c>mount-control-requires-training</c>, this
    /// entry's <c>enabledBy</c>), so the rule reads that one first and answers only where it does.
    /// What Dash, Disengage and Dodge do is the Actions table's (<c>actions-table</c>, this entry's
    /// <c>dependsOn</c>), outside the extent: a caller asking what one of them does is declined
    /// <see cref="UnresolvedReason.OutsideCurrentScope"/> citing it.
    /// </remarks>
    /// <param name="creature">Which creature the mount is, as the caller states it.</param>
    /// <param name="mount">Whether a rider is on it, as the caller states it.</param>
    /// <param name="askingWhatAnActionDoes">An action option whose rules are asked for; null when the caller asks only what the turn is.</param>
    /// <returns>The mount's turn, or a decline.</returns>
    public static Resolution<ControlledMountTurn> ControlledTurn(
        MountCreatureStatement creature,
        MountStatement mount,
        ControlledMountAction? askingWhatAnActionDoes = null)
    {
        ArgumentNullException.ThrowIfNull(creature);
        ArgumentNullException.ThrowIfNull(mount);
        if (NotRidden(MapEntries.ControlledMountTurn, mount) is { } declined)
        {
            return Resolution<ControlledMountTurn>.FromUnresolved(declined);
        }

        if (askingWhatAnActionDoes is { } asked)
        {
            Checks.Defined(asked, nameof(askingWhatAnActionDoes));
            return Declines.Of<ControlledMountTurn>(
                UnresolvedReason.OutsideCurrentScope,
                $"{Declines.Attempting(MapEntries.ControlledMountTurn)} for what {asked} does: the rule names the three actions and "
                + $"what each of them is, is the Actions table's ('{MapEntries.ActionsTable.Id}'), outside this engine's extent",
                MapEntries.ActionsTable);
        }

        return Control(creature, mount).Match(
            control => Resolution<ControlledMountTurn>.FromValue(new ControlledMountTurn(
                InitiativeMatchesTheRider: true,
                MovesOnYourTurnAsYouDirect: true,
                [ControlledMountAction.Dash, ControlledMountAction.Disengage, ControlledMountAction.Dodge],
                ActsOnTheTurnItIsMounted: true,
                control,
                MapEntries.ControlledMountTurn.Locator)),
            unresolved => Declines.Of<ControlledMountTurn>(
                unresolved.Reason,
                $"{Declines.Attempting(MapEntries.ControlledMountTurn)}: a mount is controlled only if it has been trained to accept "
                + $"a rider ('{MapEntries.MountControlRequiresTraining.Id}'), and {unresolved.Attempted}",
                MapEntries.MountControlRequiresTraining));
    }

    /// <summary>
    /// <c>independent-mount</c>: "In contrast, an independent mount—one that lets you ride but
    /// ignores your control—retains its place in the Initiative order and moves and acts as it
    /// likes."
    /// </summary>
    /// <remarks>
    /// What makes a mount independent rather than controlled is the entry's unresolved question: the
    /// corpus defines it as one that ignores your control, and does not say whether an untrained
    /// mount is always independent, whether a trained one may be, or who decides. So the rule reads
    /// the corpus's own definition as the fact the caller states, and where the caller states
    /// neither, it declines rather than deriving independence from anything else. "Moves and acts as
    /// it likes" names no decider for the mount's choices either: a caller who asks what it does is
    /// declined too.
    /// </remarks>
    /// <param name="behaviour">What the mount does with the rider's control, as the caller states it.</param>
    /// <param name="mount">Whether a rider is on it, as the caller states it.</param>
    /// <param name="askingWhatItDoes">True when the caller asks which moves and actions the mount takes.</param>
    /// <returns>What the rule says of an independent mount, or a decline.</returns>
    public static Resolution<IndependentMount> Independent(
        MountBehaviourStatement behaviour,
        MountStatement mount,
        bool askingWhatItDoes = false)
    {
        ArgumentNullException.ThrowIfNull(behaviour);
        ArgumentNullException.ThrowIfNull(mount);
        if (NotRidden(MapEntries.IndependentMount, mount) is { } declined)
        {
            return Resolution<IndependentMount>.FromUnresolved(declined);
        }

        string attempting = Declines.Attempting(MapEntries.IndependentMount);
        if (behaviour.Behaviour == MountBehaviour.NotStated)
        {
            return Declines.Of<IndependentMount>(
                UnresolvedReason.RequiresInterpretation,
                $"{attempting} while {behaviour}: the corpus defines an independent mount as one that \"lets you ride but ignores "
                + "your control\", and does not say whether an untrained mount is always independent, whether a trained mount may "
                + "be, or who decides",
                MapEntries.IndependentMount);
        }

        if (behaviour.Behaviour == MountBehaviour.TakesYourDirection)
        {
            return Resolution<IndependentMount>.FromValue(new IndependentMount(
                Independent: false,
                RetainsItsPlaceInTheInitiativeOrder: false,
                MovesAndActsAsItLikes: false,
                behaviour,
                $"the rule speaks of a mount that ignores your control, and {behaviour}; a mount that is controlled is "
                + $"'{MapEntries.ControlledMountTurn.Id}'",
                MapEntries.IndependentMount.Locator));
        }

        if (askingWhatItDoes)
        {
            return Declines.Of<IndependentMount>(
                UnresolvedReason.RequiresInterpretation,
                $"{attempting} for which moves and actions the mount takes: the rule says it \"moves and acts as it likes\" and "
                + "names no decider for those choices",
                MapEntries.IndependentMount);
        }

        return Resolution<IndependentMount>.FromValue(new IndependentMount(
            Independent: true,
            RetainsItsPlaceInTheInitiativeOrder: true,
            MovesAndActsAsItLikes: true,
            behaviour,
            $"{behaviour}, which is the corpus's own definition of an independent mount; it keeps the place "
            + $"'{MapEntries.InitiativeOrder.Id}' gave it",
            MapEntries.IndependentMount.Locator));
    }

    /// <summary>
    /// <c>falling-off</c>: "If an effect is about to move your mount against its will while you're
    /// on it, you must succeed on a DC 10 Dexterity saving throw or fall off, landing with the Prone
    /// condition … in an unoccupied space within 5 feet of the mount. While mounted, you must make
    /// the same save if you're knocked Prone or the mount is."
    /// </summary>
    /// <remarks>
    /// The saving throw is <c>saving-throws</c>' ("Playing the Game / D20 Tests / p. 6"), outside the
    /// extent, so the engine states the DC and the ability and never draws the d20: the outcome is a
    /// fact the caller states. On a success the rider stays on. On a failure the rider falls off and
    /// lands with the Prone condition, but in which unoccupied space, and what happens when there is
    /// none, is the entry's unresolved question, so the failure declines
    /// <see cref="UnresolvedReason.RequiresInterpretation"/> with what the rule did determine named
    /// in <see cref="UnresolvedResult.Attempted"/>.
    /// </remarks>
    /// <param name="trigger">Which of the rule's three cases is in play, as the caller states it.</param>
    /// <param name="outcome">What the save did, as the caller states it; null when the caller asks only what the rule demands.</param>
    /// <param name="mount">Whether a rider is on it, as the caller states it.</param>
    /// <returns>What the rule demands and what the save did, or a decline.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="trigger"/> is not stated.</exception>
    public static Resolution<FallingOffRuling> FallingOff(
        FallTrigger trigger,
        SaveOutcome? outcome,
        MountStatement mount)
    {
        ArgumentNullException.ThrowIfNull(mount);
        if (!Enum.IsDefined(trigger))
        {
            throw new ArgumentOutOfRangeException(
                nameof(trigger), trigger, "the rule states three cases, and which one is in play must be stated");
        }

        if (NotRidden(MapEntries.FallingOff, mount) is { } declined)
        {
            return Resolution<FallingOffRuling>.FromUnresolved(declined);
        }

        string demanded = trigger switch
        {
            FallTrigger.MountMovedAgainstItsWill =>
                "an effect is about to move the mount against its will while the rider is on it",
            FallTrigger.RiderKnockedProne => "the rider is knocked Prone while mounted",
            _ => "the mount is knocked Prone while the rider is on it",
        };

        if (outcome is null)
        {
            return Resolution<FallingOffRuling>.FromValue(new FallingOffRuling(
                trigger,
                FallingOffDc,
                FallingOffAbility,
                null,
                null,
                demanded,
                MapEntries.FallingOff.Locator,
                MapEntries.SavingThrows.Locator));
        }

        if (outcome.Succeeded)
        {
            return Resolution<FallingOffRuling>.FromValue(new FallingOffRuling(
                trigger,
                FallingOffDc,
                FallingOffAbility,
                outcome,
                StaysOn: true,
                $"{demanded}, and {outcome}",
                MapEntries.FallingOff.Locator,
                MapEntries.SavingThrows.Locator));
        }

        return Declines.Of<FallingOffRuling>(
            UnresolvedReason.RequiresInterpretation,
            $"{Declines.Attempting(MapEntries.FallingOff)} while {demanded}, and {outcome}: the rider falls off and lands with the "
            + $"Prone condition ('{MapEntries.ProneCondition.Id}') in an unoccupied space within {MountingReachFeet} feet of the "
            + "mount, and the corpus states neither who chooses that space nor what happens when there is none",
            MapEntries.FallingOff);
    }

    /// <summary>
    /// The decline a mounted-combat rule gives where nothing states the creature is a mount: the
    /// rules after the first apply to a mount, and <c>mount-eligibility</c> is what makes one.
    /// </summary>
    private static UnresolvedResult? NotAMount(MapEntry entry, MountStatement mount) =>
        mount.State == MountState.NotAMount
            ? new UnresolvedResult(
                UnresolvedReason.OutsideCurrentScope,
                $"{Declines.Attempting(entry)} while {mount}: the mounted-combat rules apply to a creature that serves as a mount "
                + $"('{MapEntries.MountEligibility.Id}'), and nothing states that this one does",
                MapEntries.MountEligibility.Locator)
            : null;

    /// <summary>
    /// The decline a mounted-combat rule gives where nobody is riding: a mount is controlled,
    /// independent or fallen from only once someone has mounted it, which <c>mounting-cost</c> is
    /// the rule for.
    /// </summary>
    private static UnresolvedResult? NotRidden(MapEntry entry, MountStatement mount) =>
        mount.State switch
        {
            MountState.NotAMount => NotAMount(entry, mount),
            MountState.ServesAsMount => new UnresolvedResult(
                UnresolvedReason.OutsideCurrentScope,
                $"{Declines.Attempting(entry)} while {mount}: the rule speaks of a mount someone is riding, and putting a rider on "
                + $"one is '{MapEntries.MountingCost.Id}'",
                MapEntries.MountingCost.Locator),
            _ => null,
        };
}
