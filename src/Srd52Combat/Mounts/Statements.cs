using Srd52Combat.Initiative;
using Srd52Combat.Turn;

namespace Srd52Combat.Mounts;

/// <summary>
/// How far a creature and a rider have got along "Combat / Mounted Combat / p. 15" and "Combat /
/// Mounting and Dismounting / p. 15", as the caller states it. The mounted-combat entries after the
/// first two name them in <c>enabledBy</c>: <c>mount-eligibility</c> makes a creature a mount, and
/// <c>mounting-cost</c> puts a rider on it. There is no default: <c>default</c> is refused.
/// </summary>
public enum MountState
{
    /// <summary>The creature is not a mount: neither rule has been reached for it.</summary>
    NotAMount = 1,

    /// <summary>The creature serves as a mount (<c>mount-eligibility</c>), and is not yet ridden.</summary>
    ServesAsMount = 2,

    /// <summary>A rider has mounted it (<c>mounting-cost</c>) and is on it.</summary>
    Ridden = 3,
}

/// <summary>
/// The three actions "Combat / Controlling a Mount / p. 16" allows a controlled mount, as a closed
/// set: "it has only three action options during that turn: Dash, Disengage, and Dodge". What each
/// action does is the Actions table's (<c>actions-table</c>), outside this engine's extent.
/// </summary>
public enum ControlledMountAction
{
    /// <summary>Dash.</summary>
    Dash = 1,

    /// <summary>Disengage.</summary>
    Disengage = 2,

    /// <summary>Dodge.</summary>
    Dodge = 3,
}

/// <summary>
/// What "Combat / Mounting and Dismounting / p. 15" prices: "During your move, you can mount a
/// creature that is within 5 feet of you or dismount." There is no default: <c>default</c> is
/// refused.
/// </summary>
public enum MountAction
{
    /// <summary>Mounting a creature within 5 feet of you.</summary>
    Mount = 1,

    /// <summary>Dismounting.</summary>
    Dismount = 2,
}

/// <summary>
/// Whether a mount has been trained to accept a rider, as far as the corpus's own instances settle
/// it: "Domesticated horses, mules, and similar creatures have such training"
/// (<c>mount-control-requires-training</c>, "Combat / Controlling a Mount / p. 16"). Which creature
/// is which is a fact the caller states. There is no default: <c>default</c> is refused.
/// </summary>
public enum MountCreatureKind
{
    /// <summary>A domesticated horse, which the corpus names as having such training.</summary>
    DomesticatedHorse = 1,

    /// <summary>A mule, which the corpus names as having such training.</summary>
    Mule = 2,

    /// <summary>Any other creature: the corpus names none, and says only "and similar creatures".</summary>
    AnotherCreature = 3,
}

/// <summary>
/// Whether a creature the corpus does not name has been trained to accept a rider, as the caller
/// states it. "Domesticated horses, mules, and similar creatures have such training"
/// (<c>mount-control-requires-training</c>) gives instances and no measure, and names nobody who
/// decides; Brandon ruled on 2026-09-16 that the training is a fact the caller supplies, as a Swim
/// Speed is (<c>mount-control-requires-training/training-is-stated</c>,
/// <c>docs/decisions/0007</c>). There is no default: a creature the corpus does not name, and whose
/// training nothing states, declines.
/// </summary>
public enum MountTraining
{
    /// <summary>The corpus itself names the creature as having such training; nothing need be stated, and no ruling is relied on.</summary>
    FromTheCorpus = 1,

    /// <summary>The caller states this creature has been trained to accept a rider.</summary>
    Trained = 2,

    /// <summary>The caller states this creature has not been trained to accept a rider.</summary>
    NotTrained = 3,

    /// <summary>Nothing is stated, and the engine never assumes it either way.</summary>
    NotStated = 4,
}

/// <summary>
/// Whether a creature has an anatomy appropriate to a mount, as the GM states it. "An appropriate
/// anatomy" (<c>appropriate-anatomy</c>) states no measure and no set of values, and names nobody
/// who decides; Brandon ruled on 2026-09-16 that it is the GM's call
/// (<c>appropriate-anatomy/gm-decides</c>, <c>docs/decisions/0007</c>). There is no default:
/// <c>default</c> is refused, and <see cref="NotStated"/> is how a caller says the GM has not ruled
/// on this creature.
/// </summary>
public enum AnatomyVerdict
{
    /// <summary>The GM states the creature's anatomy is appropriate.</summary>
    Appropriate = 1,

    /// <summary>The GM states it is not.</summary>
    NotAppropriate = 2,

    /// <summary>The GM has stated nothing, and the engine never assumes an anatomy.</summary>
    NotStated = 3,
}

/// <summary>
/// The GM's determination that a creature's anatomy is appropriate to a mount, or is not
/// (<c>appropriate-anatomy</c>, "Combat / Mounted Combat / p. 15"). Brandon's ruling of 2026-09-16
/// makes it the GM's call, and the caller supplies it explicitly, in the shape
/// <c>gm-requires-action</c>'s assertion has (<see cref="Turn.GmActionRequirement"/>): a party, what
/// it determined, and who is answerable. The map records no <c>assertedBy</c> for this entry — the
/// ruling is the engine's, and does not change the map — so the party is the one the map names for
/// <c>gm-requires-action</c>, which is the GM (rules-factory decision 0025).
/// </summary>
/// <param name="Decider">The party deciding, checked against the map's <c>assertedBy</c> for <c>gm-requires-action</c>.</param>
/// <param name="Verdict">What the GM determined, or that nothing is stated.</param>
/// <param name="Candidate">The creature the determination is about.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record AnatomyStatement(ActionRequirer Decider, AnatomyVerdict Verdict, string Candidate, string StatedBy)
{
    /// <summary>The party, checked against the map's <c>assertedBy</c> for <c>gm-requires-action</c>.</summary>
    public ActionRequirer Decider { get; } = ActionRequirers.Named(ActionRequirers.AssertedBy(Checks.Defined(Decider, nameof(Decider))));

    /// <summary>The verdict, checked to be stated.</summary>
    public AnatomyVerdict Verdict { get; } = Checks.Defined(Verdict, nameof(Verdict));

    /// <summary>The creature, checked to be named.</summary>
    public string Candidate { get; } = Checks.Text(Candidate, nameof(Candidate));

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The GM determines that this creature's anatomy is appropriate.</summary>
    /// <param name="candidate">The creature.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static AnatomyStatement Appropriate(string candidate, string statedBy) =>
        new(ActionRequirer.Gm, AnatomyVerdict.Appropriate, candidate, statedBy);

    /// <summary>The GM determines that this creature's anatomy is not appropriate.</summary>
    /// <param name="candidate">The creature.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static AnatomyStatement NotAppropriate(string candidate, string statedBy) =>
        new(ActionRequirer.Gm, AnatomyVerdict.NotAppropriate, candidate, statedBy);

    /// <summary>The GM has determined nothing about this creature's anatomy.</summary>
    /// <param name="candidate">The creature.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static AnatomyStatement NotStated(string candidate, string statedBy) =>
        new(ActionRequirer.Gm, AnatomyVerdict.NotStated, candidate, statedBy);

    /// <inheritdoc/>
    public override string ToString() => Verdict switch
    {
        AnatomyVerdict.Appropriate => $"the {ActionRequirers.AssertedBy(Decider)} determines that {Candidate} has an appropriate anatomy, as stated by {StatedBy}",
        AnatomyVerdict.NotAppropriate => $"the {ActionRequirers.AssertedBy(Decider)} determines that {Candidate} does not have an appropriate anatomy, as stated by {StatedBy}",
        _ => $"the {ActionRequirers.AssertedBy(Decider)} has determined nothing about {Candidate}'s anatomy, as stated by {StatedBy}",
    };
}

/// <summary>
/// Whether a mount takes the rider's direction, as the caller states it: an independent mount is
/// "one that lets you ride but ignores your control" (<c>independent-mount</c>, "Combat /
/// Controlling a Mount / p. 16"). There is no default: <c>default</c> is refused.
/// </summary>
public enum MountBehaviour
{
    /// <summary>The mount ignores the rider's control, which is the corpus's own definition of an independent mount.</summary>
    IgnoresYourControl = 1,

    /// <summary>The mount takes the rider's direction.</summary>
    TakesYourDirection = 2,

    /// <summary>The caller states neither: what the mount does with the rider's control is not said.</summary>
    NotStated = 3,
}

/// <summary>
/// Which of the sentences of "Combat / Falling Off / p. 16" is in play, as the caller states it.
/// There is no default: <c>default</c> is refused.
/// </summary>
public enum FallTrigger
{
    /// <summary>"If an effect is about to move your mount against its will while you're on it".</summary>
    MountMovedAgainstItsWill = 1,

    /// <summary>"you're knocked Prone".</summary>
    RiderKnockedProne = 2,

    /// <summary>"or the mount is".</summary>
    MountKnockedProne = 3,
}

/// <summary>
/// Whether a rider is riding a mount, as the caller states it: the <c>enabledBy</c> gate the
/// mounted-combat entries name. Whether a creature can serve as a mount turns on
/// <c>appropriate-anatomy</c>, whose question the corpus leaves open, so the engine cannot reach
/// these rules by deciding it and is told instead who is on what (rules-factory decision 0025, and
/// decision 0005 of this engine).
/// </summary>
/// <param name="State">How far along the mounted-combat rules the creature and rider are.</param>
/// <param name="Mount">The mount's name or handle.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record MountStatement(MountState State, string Mount, string StatedBy)
{
    /// <summary>The state, checked to be stated.</summary>
    public MountState State { get; } = Checks.Defined(State, nameof(State));

    /// <summary>The mount, checked to be named.</summary>
    public string Mount { get; } = Checks.Text(Mount, nameof(Mount));

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The creature is not a mount.</summary>
    /// <param name="mount">The creature's name or handle.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static MountStatement NotAMount(string mount, string statedBy) =>
        new(MountState.NotAMount, mount, statedBy);

    /// <summary>The creature serves as a mount, and no rider is on it yet.</summary>
    /// <param name="mount">The mount's name or handle.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static MountStatement Serves(string mount, string statedBy) =>
        new(MountState.ServesAsMount, mount, statedBy);

    /// <summary>A rider has mounted it and is on it.</summary>
    /// <param name="mount">The mount's name or handle.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static MountStatement Ridden(string mount, string statedBy) =>
        new(MountState.Ridden, mount, statedBy);

    /// <inheritdoc/>
    public override string ToString() => State switch
    {
        MountState.NotAMount => $"{Mount} is not a mount, as stated by {StatedBy}",
        MountState.ServesAsMount => $"{Mount} serves as a mount and is not yet ridden, as stated by {StatedBy}",
        _ => $"{Mount} serves as a mount and is ridden, as stated by {StatedBy}",
    };
}

/// <summary>
/// Whether a creature is willing to serve as a mount, as the caller states it: "A willing creature
/// …" (<c>mount-eligibility</c>). Willingness is a fact of play and the engine never infers one.
/// </summary>
/// <param name="Willing">True when the creature is willing.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record WillingStatement(bool Willing, string StatedBy)
{
    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The creature is willing.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static WillingStatement Is(string statedBy) => new(Willing: true, statedBy);

    /// <summary>The creature is not willing.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static WillingStatement IsNot(string statedBy) => new(Willing: false, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        Willing
            ? $"the creature is willing, as stated by {StatedBy}"
            : $"the creature is not willing, as stated by {StatedBy}";
}

/// <summary>
/// Which creature the mount is, as the caller states it: the corpus names domesticated horses and
/// mules as having the training a controlled mount needs.
/// </summary>
/// <param name="Kind">Which of the corpus's instances it is, or another creature.</param>
/// <param name="Id">The mount's name or handle.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
/// <param name="Training">
/// Whether the caller states this creature has been trained to accept a rider. For one of the
/// corpus's own instances it is <see cref="MountTraining.FromTheCorpus"/> and nothing is stated; for
/// any other creature it is the caller's fact, under Brandon's ruling of 2026-09-16, and
/// <see cref="MountTraining.NotStated"/> where the caller says nothing.
/// </param>
public sealed record MountCreatureStatement(MountCreatureKind Kind, string Id, string StatedBy, MountTraining Training)
{
    /// <summary>The kind, checked to be stated.</summary>
    public MountCreatureKind Kind { get; } = Checks.Defined(Kind, nameof(Kind));

    /// <summary>The mount, checked to be named.</summary>
    public string Id { get; } = Checks.Text(Id, nameof(Id));

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The training, checked to be stated, and to be the corpus's only for a creature the corpus names.</summary>
    public MountTraining Training { get; } = CheckTraining(Kind, Training);

    /// <summary>A domesticated horse, one of the corpus's own instances.</summary>
    /// <param name="id">The mount's name or handle.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static MountCreatureStatement DomesticatedHorse(string id, string statedBy) =>
        new(MountCreatureKind.DomesticatedHorse, id, statedBy, MountTraining.FromTheCorpus);

    /// <summary>A mule, one of the corpus's own instances.</summary>
    /// <param name="id">The mount's name or handle.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static MountCreatureStatement Mule(string id, string statedBy) =>
        new(MountCreatureKind.Mule, id, statedBy, MountTraining.FromTheCorpus);

    /// <summary>A creature the corpus does not name, whose training nothing states.</summary>
    /// <param name="id">The mount's name or handle.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static MountCreatureStatement AnotherCreature(string id, string statedBy) =>
        new(MountCreatureKind.AnotherCreature, id, statedBy, MountTraining.NotStated);

    /// <summary>A creature the corpus does not name, whose training the caller states.</summary>
    /// <param name="id">The mount's name or handle.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <param name="trained">True when the caller states it has been trained to accept a rider.</param>
    /// <returns>The statement.</returns>
    public static MountCreatureStatement AnotherCreature(string id, string statedBy, bool trained) =>
        new(MountCreatureKind.AnotherCreature, id, statedBy, trained ? MountTraining.Trained : MountTraining.NotTrained);

    /// <summary>True for the creatures the corpus names as having such training.</summary>
    public bool NamedByTheCorpus => Kind != MountCreatureKind.AnotherCreature;

    private static MountTraining CheckTraining(MountCreatureKind kind, MountTraining training)
    {
        bool named = Checks.Defined(kind, nameof(Kind)) != MountCreatureKind.AnotherCreature;
        if (named != (Checks.Defined(training, nameof(Training)) == MountTraining.FromTheCorpus))
        {
            throw new ArgumentException(
                "a domesticated horse and a mule are trained by the corpus's own words, and no other creature is: "
                + "any other creature's training is the caller's to state, or to leave unstated",
                nameof(Training));
        }

        return training;
    }

    /// <inheritdoc/>
    public override string ToString() => Kind switch
    {
        MountCreatureKind.DomesticatedHorse => $"{Id} is a domesticated horse, as stated by {StatedBy}",
        MountCreatureKind.Mule => $"{Id} is a mule, as stated by {StatedBy}",
        _ => Training switch
        {
            MountTraining.Trained => $"{Id} is a creature the corpus does not name, trained to accept a rider, as stated by {StatedBy}",
            MountTraining.NotTrained => $"{Id} is a creature the corpus does not name, not trained to accept a rider, as stated by {StatedBy}",
            _ => $"{Id} is a creature the corpus does not name, whose training {StatedBy} does not state",
        },
    };
}

/// <summary>
/// What the mount does with the rider's control, as the caller states it.
/// </summary>
/// <param name="Behaviour">Whether it ignores the rider's control, takes direction, or neither is stated.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record MountBehaviourStatement(MountBehaviour Behaviour, string StatedBy)
{
    /// <summary>The behaviour, checked to be stated.</summary>
    public MountBehaviour Behaviour { get; } = Checks.Defined(Behaviour, nameof(Behaviour));

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The mount ignores the rider's control.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static MountBehaviourStatement IgnoresYourControl(string statedBy) =>
        new(MountBehaviour.IgnoresYourControl, statedBy);

    /// <summary>The mount takes the rider's direction.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static MountBehaviourStatement TakesYourDirection(string statedBy) =>
        new(MountBehaviour.TakesYourDirection, statedBy);

    /// <summary>Nothing is stated about what the mount does with the rider's control.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static MountBehaviourStatement NotStated(string statedBy) =>
        new(MountBehaviour.NotStated, statedBy);

    /// <inheritdoc/>
    public override string ToString() => Behaviour switch
    {
        MountBehaviour.IgnoresYourControl => $"the mount lets you ride but ignores your control, as stated by {StatedBy}",
        MountBehaviour.TakesYourDirection => $"the mount takes the rider's direction, as stated by {StatedBy}",
        _ => $"what the mount does with the rider's control is not stated by {StatedBy}",
    };
}

/// <summary>
/// The outcome of the DC 10 Dexterity saving throw "Combat / Falling Off / p. 16" calls for, as the
/// caller states it. Making a saving throw is <c>saving-throws</c> ("Playing the Game / D20 Tests /
/// p. 6"), outside this engine's extent, so the engine states the DC and the ability, never draws
/// the d20, and is told what the save did.
/// </summary>
/// <param name="Succeeded">True when the rider succeeded on the save.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record SaveOutcome(bool Succeeded, string StatedBy)
{
    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The rider succeeded on the save.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static SaveOutcome Succeeds(string statedBy) => new(Succeeded: true, statedBy);

    /// <summary>The rider failed the save.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static SaveOutcome Fails(string statedBy) => new(Succeeded: false, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        Succeeded
            ? $"the rider succeeded on the saving throw, as stated by {StatedBy}"
            : $"the rider failed the saving throw, as stated by {StatedBy}";
}
