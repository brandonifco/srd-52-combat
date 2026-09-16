using System.Collections.Immutable;
using Srd52Combat.Initiative;

namespace Srd52Combat.Attacks;

/// <summary>
/// What an attack can be made from, as "Combat / Making an Attack / p. 14" names them: the Attack
/// action, and the other actions, Bonus Actions and Reactions that also let you make one. Which
/// other ones do is <c>actions-table</c>, <c>bonus-actions</c> and <c>reactions</c>, outside this
/// engine's slice, so only the Attack action is answered here. There is no default:
/// <c>default</c> is refused.
/// </summary>
public enum AttackSource
{
    /// <summary>The Attack action, which the corpus says makes an attack.</summary>
    AttackAction = 1,

    /// <summary>Another action: whether it lets you make an attack is the Actions table's.</summary>
    OtherAction = 2,

    /// <summary>A Bonus Action: whether it lets you make an attack is <c>bonus-actions</c>'.</summary>
    BonusAction = 3,

    /// <summary>A Reaction: whether it lets you make an attack is <c>reactions</c>'.</summary>
    Reaction = 4,
}

/// <summary>
/// What an attack may be aimed at, "Combat / Making an Attack / p. 15": "a creature, an object, or
/// a location". The set is closed at three. There is no default: <c>default</c> is refused.
/// </summary>
public enum TargetKind
{
    /// <summary>A creature.</summary>
    Creature = 1,

    /// <summary>An object.</summary>
    Object = 2,

    /// <summary>A location.</summary>
    Location = 3,
}

/// <summary>
/// Which range rule an attack is measured by, as the caller states the attack. Each is its own map
/// entry, and only <c>normal-and-long-range</c> is built here. There is no default:
/// <c>default</c> is refused.
/// </summary>
public enum AttackRangeKind
{
    /// <summary>A melee attack, measured by the attacker's reach (<c>melee-within-reach</c>).</summary>
    Melee = 1,

    /// <summary>A ranged attack with one range (<c>single-range</c>).</summary>
    RangedSingleRange = 2,

    /// <summary>A ranged attack with a normal and a long range (<c>normal-and-long-range</c>).</summary>
    RangedTwoRanges = 3,
}

/// <summary>
/// What a rule in this slice does to an attack roll. How Advantage and Disadvantage then resolve,
/// and what happens when both apply, is <c>advantage-disadvantage</c> ("Playing the Game /
/// Advantage/Disadvantage / p. 7"), outside the slice: this engine names the effect a rule gives
/// and never combines two of them.
/// </summary>
public enum RollEffect
{
    /// <summary>The rule gives the roll neither Advantage nor Disadvantage.</summary>
    None = 1,

    /// <summary>The rule gives the roll Advantage.</summary>
    Advantage = 2,

    /// <summary>The rule gives the roll Disadvantage.</summary>
    Disadvantage = 3,
}

/// <summary>
/// How much of the target the attacker has, as "Combat / Unseen Attackers and Targets / p. 14"
/// distinguishes it: seen, heard but not seen, or not perceived at all so that its location is
/// guessed. The last two are the sentence's own two cases of a target you can't see. Vision itself
/// is "Exploration" (p. 11), outside the extent, so which one holds is a fact the caller states.
/// There is no default: <c>default</c> is refused.
/// </summary>
public enum TargetVisibility
{
    /// <summary>The attacker can see the target.</summary>
    Seen = 1,

    /// <summary>The attacker can hear the target but not see it.</summary>
    HeardNotSeen = 2,

    /// <summary>The attacker cannot see the target and is guessing its location.</summary>
    LocationGuessed = 3,
}

/// <summary>
/// How a creature leaves the attacker's reach, as "Combat / Opportunity Attacks / p. 15" and the
/// caller state it. The first four are the means the Rules Glossary's Opportunity Attacks entry
/// lists ("using its action, its Bonus Action, its Reaction, or one of its speeds", p. 185);
/// <see cref="Teleport"/> and <see cref="MovedWithoutItsOwn"/> are the avoidance sentence's own
/// cases; <see cref="ByNoneOfThose"/> is a creature that leaves reach by none of them, which the
/// slice does not cover. There is no default: <c>default</c> is refused.
/// </summary>
public enum DepartureMeans
{
    /// <summary>It uses its own movement, one of its speeds.</summary>
    OwnMovement = 1,

    /// <summary>It uses its action.</summary>
    OwnAction = 2,

    /// <summary>It uses its Bonus Action.</summary>
    OwnBonusAction = 3,

    /// <summary>It uses its Reaction.</summary>
    OwnReaction = 4,

    /// <summary>It Teleports.</summary>
    Teleport = 5,

    /// <summary>It is moved without using its movement, action, Bonus Action, or Reaction.</summary>
    MovedWithoutItsOwn = 6,

    /// <summary>It leaves reach by none of those, for instance because the attacker moved away.</summary>
    ByNoneOfThose = 7,
}

/// <summary>
/// The two ranges of a ranged attack that has them, "Combat / Range / p. 15": "The smaller number
/// is the normal range, and the larger number is the long range."
/// </summary>
/// <param name="NormalFeet">The normal range, in feet.</param>
/// <param name="LongFeet">The long range, in feet; larger than the normal range.</param>
public sealed record TwoRanges(int NormalFeet, int LongFeet)
{
    /// <summary>The normal range, checked to be positive.</summary>
    public int NormalFeet { get; } = NormalFeet > 0
        ? NormalFeet
        : throw new ArgumentOutOfRangeException(nameof(NormalFeet), NormalFeet, "a normal range is a positive number of feet");

    /// <summary>The long range, checked to be larger than the normal range.</summary>
    public int LongFeet { get; } = LongFeet > NormalFeet
        ? LongFeet
        : throw new ArgumentOutOfRangeException(nameof(LongFeet), LongFeet, "the larger number is the long range, so it exceeds the normal range");

    /// <inheritdoc/>
    public override string ToString() => $"normal range {NormalFeet} ft, long range {LongFeet} ft";
}

/// <summary>
/// Whether the attacker can see the target, as the caller states it. Vision is "Exploration"
/// (p. 11), outside the extent, so the engine demands the statement, records it, and never infers
/// one in either direction.
/// </summary>
/// <param name="Visibility">Seen, heard but not seen, or a guessed location.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record TargetVisibilityStatement(TargetVisibility Visibility, string StatedBy)
{
    /// <summary>The visibility, checked to be stated.</summary>
    public TargetVisibility Visibility { get; } = Checks.Defined(Visibility, nameof(Visibility));

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The attacker can see the target.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static TargetVisibilityStatement Seen(string statedBy) => new(TargetVisibility.Seen, statedBy);

    /// <summary>The attacker can hear the target but not see it.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static TargetVisibilityStatement HeardNotSeen(string statedBy) => new(TargetVisibility.HeardNotSeen, statedBy);

    /// <summary>The attacker cannot see the target and is guessing its location.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static TargetVisibilityStatement LocationGuessed(string statedBy) => new(TargetVisibility.LocationGuessed, statedBy);

    /// <summary>True while the attacker cannot see the target, whichever of the two cases holds.</summary>
    public bool CannotSee => Visibility != TargetVisibility.Seen;

    /// <inheritdoc/>
    public override string ToString() => Visibility switch
    {
        TargetVisibility.Seen => $"the attacker can see the target, as stated by {StatedBy}",
        TargetVisibility.HeardNotSeen => $"the attacker can hear the target but not see it, as stated by {StatedBy}",
        _ => $"the attacker cannot see the target and is guessing its location, as stated by {StatedBy}",
    };
}

/// <summary>
/// Whether the target can see the attacker, as the caller states it. The other direction of
/// <see cref="TargetVisibilityStatement"/>, and the fact <c>unseen-attacker-advantage</c> tests.
/// </summary>
/// <param name="CanSeeYou">True when the creature can see the attacker.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record AttackerSeenStatement(bool CanSeeYou, string StatedBy)
{
    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The creature can see the attacker.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static AttackerSeenStatement Seen(string statedBy) => new(CanSeeYou: true, statedBy);

    /// <summary>The creature cannot see the attacker.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static AttackerSeenStatement Unseen(string statedBy) => new(CanSeeYou: false, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        CanSeeYou
            ? $"the creature can see the attacker, as stated by {StatedBy}"
            : $"the creature cannot see the attacker, as stated by {StatedBy}";
}

/// <summary>
/// Whether the attacker is hidden, as the caller states it. Being hidden is the Hide action's
/// (<c>actions-table</c>) and the Rules Glossary's, outside the extent.
/// </summary>
/// <param name="Hidden">True when the attacker is hidden.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record HiddenStatement(bool Hidden, string StatedBy)
{
    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The attacker is hidden.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static HiddenStatement IsHidden(string statedBy) => new(Hidden: true, statedBy);

    /// <summary>The attacker is not hidden.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static HiddenStatement NotHidden(string statedBy) => new(Hidden: false, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        Hidden
            ? $"the attacker is hidden, as stated by {StatedBy}"
            : $"the attacker is not hidden, as stated by {StatedBy}";
}

/// <summary>
/// Whether a creature has the Incapacitated condition, as the caller states it. The condition is
/// <c>incapacitated-condition</c> ("Rules Glossary / p. 184"), outside the slice; what the rules
/// here test is only whether it holds, which the caller states and the engine never infers.
/// </summary>
/// <param name="Incapacitated">True when the creature has the condition.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record IncapacitatedStatement(bool Incapacitated, string StatedBy)
{
    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The creature has the Incapacitated condition.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static IncapacitatedStatement Is(string statedBy) => new(Incapacitated: true, statedBy);

    /// <summary>The creature does not have the Incapacitated condition.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static IncapacitatedStatement IsNot(string statedBy) => new(Incapacitated: false, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        Incapacitated
            ? $"the creature has the Incapacitated condition, as stated by {StatedBy}"
            : $"the creature does not have the Incapacitated condition, as stated by {StatedBy}";
}

/// <summary>
/// One enemy within 5 feet of the attacker, as the caller states it. Whether a creature is an
/// enemy, and whether it can see the attacker, are parameters; the Incapacitated condition is
/// <c>incapacitated-condition</c>'s, outside the slice.
/// </summary>
/// <param name="Id">The enemy's name or handle.</param>
/// <param name="CanSeeYou">True when it can see the attacker.</param>
/// <param name="Incapacitated">True when it has the Incapacitated condition.</param>
public sealed record NearbyEnemy(string Id, bool CanSeeYou, bool Incapacitated)
{
    /// <summary>The enemy, checked to be non-empty.</summary>
    public string Id { get; } = Checks.Text(Id, nameof(Id));

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Id} ({(CanSeeYou ? "can see you" : "cannot see you")}, {(Incapacitated ? "Incapacitated" : "not Incapacitated")})";
}

/// <summary>
/// Every enemy within 5 feet of the attacker, as the caller states them. The engine demands the
/// statement and never infers one: an empty list is the caller saying there are none, not the
/// absence of a statement.
/// </summary>
/// <param name="Enemies">The enemies within 5 feet; empty when the caller states there are none.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record EnemiesWithinFiveFeet(ImmutableArray<NearbyEnemy> Enemies, string StatedBy)
{
    /// <summary>The enemies, checked to be present and distinct.</summary>
    public ImmutableArray<NearbyEnemy> Enemies { get; } = CheckEnemies(Enemies);

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>No enemy is within 5 feet of the attacker.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static EnemiesWithinFiveFeet None(string statedBy) => new([], statedBy);

    /// <summary>These enemies are within 5 feet of the attacker.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <param name="enemies">The enemies.</param>
    /// <returns>The statement.</returns>
    public static EnemiesWithinFiveFeet Of(string statedBy, params NearbyEnemy[] enemies) =>
        new([.. enemies ?? throw new ArgumentNullException(nameof(enemies))], statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        Enemies.IsEmpty
            ? $"no enemy is within 5 feet of the attacker, as stated by {StatedBy}"
            : $"within 5 feet of the attacker: {string.Join(", ", Enemies)}, as stated by {StatedBy}";

    private static ImmutableArray<NearbyEnemy> CheckEnemies(ImmutableArray<NearbyEnemy> enemies)
    {
        if (enemies.IsDefault || enemies.Any(e => e is null))
        {
            throw new ArgumentException("the enemies within 5 feet must be stated, and none is null", nameof(Enemies));
        }

        return enemies.Select(e => e.Id).Distinct(StringComparer.Ordinal).Count() == enemies.Length
            ? enemies
            : throw new ArgumentException("every enemy within 5 feet has its own id", nameof(Enemies));
    }
}

/// <summary>
/// Whether the attack roll hit, as the caller states it. Making the roll and reading it against a
/// target is <c>attack-rolls</c> ("Playing the Game / D20 Tests / p. 7"), outside the slice, so the
/// engine demands the outcome, records who it came from, and never draws or infers it.
/// </summary>
/// <param name="Hit">True when the attack roll hit.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record AttackRollOutcome(bool Hit, string StatedBy)
{
    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The attack roll hit.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static AttackRollOutcome Hits(string statedBy) => new(Hit: true, statedBy);

    /// <summary>The attack roll missed.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static AttackRollOutcome Misses(string statedBy) => new(Hit: false, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        Hit
            ? $"the attack roll hit, as stated by {StatedBy}"
            : $"the attack roll missed, as stated by {StatedBy}";
}

/// <summary>
/// Whether the particular attack has rules of its own that specify otherwise than rolling damage on
/// a hit, as the caller states it. Those rules belong to the attack, outside the extent, so the
/// engine demands the statement and never infers one.
/// </summary>
/// <param name="SpecifiesOtherwise">True when the attack's own rules specify otherwise.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record AttackDamageRules(bool SpecifiesOtherwise, string StatedBy)
{
    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The attack has no rules specifying otherwise; damage is rolled on a hit.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static AttackDamageRules Ordinary(string statedBy) => new(SpecifiesOtherwise: false, statedBy);

    /// <summary>The attack's own rules specify otherwise.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static AttackDamageRules SpecifyOtherwise(string statedBy) => new(SpecifiesOtherwise: true, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        SpecifiesOtherwise
            ? $"the attack has rules of its own that specify otherwise, as stated by {StatedBy}"
            : $"the attack has no rules specifying otherwise, as stated by {StatedBy}";
}

/// <summary>
/// The location the attacker targeted and whether the target is in it, as the caller states it.
/// Where a creature is, is the table's fact, so the engine demands it and never infers one.
/// </summary>
/// <param name="TargetedLocation">The location the attacker picked.</param>
/// <param name="TargetIsThere">True when the target is in that location.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record TargetLocationStatement(string TargetedLocation, bool TargetIsThere, string StatedBy)
{
    /// <summary>The location, checked to be non-empty.</summary>
    public string TargetedLocation { get; } = Checks.Text(TargetedLocation, nameof(TargetedLocation));

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <inheritdoc/>
    public override string ToString() =>
        TargetIsThere
            ? $"the target is in {TargetedLocation}, the location the attacker targeted, as stated by {StatedBy}"
            : $"the target is not in {TargetedLocation}, the location the attacker targeted, as stated by {StatedBy}";
}

/// <summary>
/// How a creature left the attacker's reach, and whether it took the Disengage action, as the
/// caller states it. The Disengage action is <c>actions-table</c>'s and <c>disengage-action</c>'s,
/// outside the extent; what the rules here test is only that it was taken.
/// </summary>
/// <param name="Means">How the creature left reach.</param>
/// <param name="DisengageTaken">True when the creature took the Disengage action.</param>
/// <param name="Description">What happened, in the caller's words; the corpus's own examples are an explosion hurling you and falling past an enemy.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record LeavingReach(DepartureMeans Means, bool DisengageTaken, string Description, string StatedBy)
{
    /// <summary>The means, checked to be stated.</summary>
    public DepartureMeans Means { get; } = Checks.Defined(Means, nameof(Means));

    /// <summary>What happened, checked to be non-empty.</summary>
    public string Description { get; } = Checks.Text(Description, nameof(Description));

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The creature moved out of reach using its own movement, taking no Disengage action.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <param name="description">What happened, in the caller's words.</param>
    /// <returns>The statement.</returns>
    public static LeavingReach Walks(string statedBy, string description = "the creature walks out of the attacker's reach") =>
        new(DepartureMeans.OwnMovement, DisengageTaken: false, description, statedBy);

    /// <summary>The creature took the Disengage action and then moved out of reach.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <param name="description">What happened, in the caller's words.</param>
    /// <returns>The statement.</returns>
    public static LeavingReach Disengages(string statedBy, string description = "the creature takes the Disengage action and moves out of the attacker's reach") =>
        new(DepartureMeans.OwnMovement, DisengageTaken: true, description, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Description} ({Means}{(DisengageTaken ? ", having taken the Disengage action" : string.Empty)}), as stated by {StatedBy}";
}

/// <summary>
/// Whether the attacker still has its Reaction, as the caller states it. How many Reactions a
/// creature has and when they come back is <c>reactions</c> ("Playing the Game / p. 10"), outside
/// the slice, so the engine demands the statement and never infers one.
/// </summary>
/// <param name="Available">True when the attacker has not taken a Reaction since the start of its last turn.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record ReactionAvailability(bool Available, string StatedBy)
{
    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The attacker still has its Reaction.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static ReactionAvailability Has(string statedBy) => new(Available: true, statedBy);

    /// <summary>The attacker has already taken a Reaction since the start of its last turn.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static ReactionAvailability AlreadyTaken(string statedBy) => new(Available: false, statedBy);

    /// <inheritdoc/>
    public override string ToString() =>
        Available
            ? $"the attacker still has its Reaction, as stated by {StatedBy}"
            : $"the attacker has already taken a Reaction since the start of its last turn, as stated by {StatedBy}";
}
