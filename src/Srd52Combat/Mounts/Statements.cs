using Srd52Combat.Initiative;

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
public sealed record MountCreatureStatement(MountCreatureKind Kind, string Id, string StatedBy)
{
    /// <summary>The kind, checked to be stated.</summary>
    public MountCreatureKind Kind { get; } = Checks.Defined(Kind, nameof(Kind));

    /// <summary>The mount, checked to be named.</summary>
    public string Id { get; } = Checks.Text(Id, nameof(Id));

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>A domesticated horse, one of the corpus's own instances.</summary>
    /// <param name="id">The mount's name or handle.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static MountCreatureStatement DomesticatedHorse(string id, string statedBy) =>
        new(MountCreatureKind.DomesticatedHorse, id, statedBy);

    /// <summary>A mule, one of the corpus's own instances.</summary>
    /// <param name="id">The mount's name or handle.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static MountCreatureStatement Mule(string id, string statedBy) =>
        new(MountCreatureKind.Mule, id, statedBy);

    /// <summary>A creature the corpus does not name.</summary>
    /// <param name="id">The mount's name or handle.</param>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static MountCreatureStatement AnotherCreature(string id, string statedBy) =>
        new(MountCreatureKind.AnotherCreature, id, statedBy);

    /// <summary>True for the creatures the corpus names as having such training.</summary>
    public bool NamedByTheCorpus => Kind != MountCreatureKind.AnotherCreature;

    /// <inheritdoc/>
    public override string ToString() => Kind switch
    {
        MountCreatureKind.DomesticatedHorse => $"{Id} is a domesticated horse, as stated by {StatedBy}",
        MountCreatureKind.Mule => $"{Id} is a mule, as stated by {StatedBy}",
        _ => $"{Id} is a creature the corpus does not name, as stated by {StatedBy}",
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
