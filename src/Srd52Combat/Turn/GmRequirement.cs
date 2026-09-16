using System.Collections.Immutable;
using Srd52Combat.Initiative;

namespace Srd52Combat.Turn;

/// <summary>
/// Who may require an action for an activity: <c>gm-requires-action</c>'s <c>assertedBy</c>, "GM"
/// (rules-factory decision 0025). As with an Initiative tie's decider (decision 0002), the party is
/// checked against the map's list and not against a list of the engine's own.
/// </summary>
public enum ActionRequirer
{
    /// <summary>The GM, who "might require you to use an action for any of these activities".</summary>
    Gm = 1,
}

/// <summary>The parties the map lets assert <c>gm-requires-action</c>.</summary>
public static class ActionRequirers
{
    /// <summary>Every party a requirement may name: one per party in the map's <c>assertedBy</c> for <c>gm-requires-action</c>, in the map's order.</summary>
    public static ImmutableArray<ActionRequirer> Allowed { get; } =
        [.. MapEntries.GmRequiresAction.AssertedBy.Select(Named)];

    /// <summary>The party <paramref name="requirer"/> is, in the words of the map's <c>assertedBy</c>.</summary>
    /// <param name="requirer">The party.</param>
    /// <returns>"GM".</returns>
    public static string AssertedBy(ActionRequirer requirer) => requirer switch
    {
        ActionRequirer.Gm => "GM",
        _ => throw new ArgumentOutOfRangeException(nameof(requirer), requirer, "not a party"),
    };

    /// <summary>The party for <paramref name="party"/>, which the map's <c>assertedBy</c> for <c>gm-requires-action</c> must name.</summary>
    /// <param name="party">A party, in the map's words.</param>
    /// <returns>The party.</returns>
    /// <exception cref="InvalidOperationException">The map does not name the party, or names one the engine has no party for.</exception>
    public static ActionRequirer Named(string party)
    {
        if (!MapEntries.GmRequiresAction.AssertedBy.Contains(party, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                $"'{MapEntries.GmRequiresAction.Id}' is asserted by {string.Join(", ", MapEntries.GmRequiresAction.AssertedBy)} [map assertedBy], not {party}");
        }

        var matching = Enum.GetValues<ActionRequirer>().Where(r => AssertedBy(r) == party).ToArray();
        return matching.Length == 1
            ? matching[0]
            : throw new InvalidOperationException($"the map's assertedBy for '{MapEntries.GmRequiresAction.Id}' names {party}, and the engine has no party for that");
    }
}

/// <summary>
/// What kind of activity a requirement names. "Any of these activities" has no stated antecedent;
/// the map resolves the pointer to the two entries that state a free activity on a turn,
/// <c>free-object-interaction</c> and <c>communication-cost</c> (<c>crossReferences</c>). Whether
/// it reaches communication at all is <c>communication-cost</c>'s unresolved question, which is
/// asked there and not here. There is no default: <c>default</c> is refused.
/// </summary>
public enum ActivityKind
{
    /// <summary>Interacting with an object or a feature of the environment (<c>free-object-interaction</c>, p. 13).</summary>
    ObjectInteraction = 1,

    /// <summary>Communicating (<c>communication-cost</c>, p. 13).</summary>
    Communication = 2,
}

/// <summary>One activity the GM requires an action for, named as the caller names it elsewhere.</summary>
/// <param name="Kind">An object interaction, or communication.</param>
/// <param name="What">The activity, in the same words the interaction or the communication uses.</param>
public sealed record RequiredActivity(ActivityKind Kind, string What)
{
    /// <summary>The kind, checked to be stated.</summary>
    public ActivityKind Kind { get; } = Checks.Defined(Kind, nameof(Kind));

    /// <summary>The activity, checked to be non-empty.</summary>
    public string What { get; } = Checks.Text(What, nameof(What));

    /// <summary>An object interaction the GM requires an action for.</summary>
    /// <param name="what">The interaction, in the caller's words.</param>
    /// <returns>The activity.</returns>
    public static RequiredActivity Interaction(string what) => new(ActivityKind.ObjectInteraction, what);

    /// <summary>A communication the GM requires an action for.</summary>
    /// <param name="what">The communication, in the caller's words.</param>
    /// <returns>The activity.</returns>
    public static RequiredActivity Communication(string what) => new(ActivityKind.Communication, what);

    /// <inheritdoc/>
    public override string ToString() => $"{What} ({Kind})";
}

/// <summary>
/// The value of the assertion <c>gm-requires-action</c> ("Combat / Your Turn / p. 14"): the
/// activities the GM has determined need special care or present an unusual obstacle, and so
/// require an action. The measure is the GM's ("when it needs special care or when it presents an
/// unusual obstacle"), and the engine never applies it: it takes the determination, attributed, and
/// never infers one in either direction.
/// </summary>
/// <param name="Requirer">The party requiring it, which the map's <c>assertedBy</c> must name.</param>
/// <param name="Activities">The activities an action is required for; empty when the GM requires none.</param>
/// <param name="StatedBy">Who is answerable for the statement.</param>
public sealed record GmActionRequirement(ActionRequirer Requirer, ImmutableArray<RequiredActivity> Activities, string StatedBy)
{
    /// <summary>The party, checked against the map's <c>assertedBy</c> for <c>gm-requires-action</c>.</summary>
    public ActionRequirer Requirer { get; } = ActionRequirers.Named(ActionRequirers.AssertedBy(Checks.Defined(Requirer, nameof(Requirer))));

    /// <summary>The activities, checked to be present and each stated.</summary>
    public ImmutableArray<RequiredActivity> Activities { get; } =
        Activities.IsDefault || Activities.Any(a => a is null)
            ? throw new ArgumentException("the activities an action is required for must be stated, even when there are none", nameof(Activities))
            : Activities;

    /// <summary>Who stated it, checked to be non-empty.</summary>
    public string StatedBy { get; } = Checks.Text(StatedBy, nameof(StatedBy));

    /// <summary>The GM requires an action for no activity on this turn.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <returns>The statement.</returns>
    public static GmActionRequirement None(string statedBy) => new(ActionRequirer.Gm, [], statedBy);

    /// <summary>The GM requires an action for these activities.</summary>
    /// <param name="statedBy">Who is answerable for the statement.</param>
    /// <param name="activities">The activities.</param>
    /// <returns>The statement.</returns>
    public static GmActionRequirement Of(string statedBy, params RequiredActivity[] activities) =>
        new(ActionRequirer.Gm, [.. activities], statedBy);

    /// <summary>Whether the GM requires an action for <paramref name="what"/> as an activity of <paramref name="kind"/>.</summary>
    /// <param name="kind">An object interaction, or communication.</param>
    /// <param name="what">The activity, in the words the caller used for it.</param>
    /// <returns>True when this statement names it.</returns>
    public bool Requires(ActivityKind kind, string what) =>
        Activities.Any(a => a.Kind == kind && string.Equals(a.What, what, StringComparison.Ordinal));

    /// <inheritdoc/>
    public override string ToString() =>
        Activities.IsEmpty
            ? $"the {ActionRequirers.AssertedBy(Requirer)} requires an action for no activity, as stated by {StatedBy}"
            : $"the {ActionRequirers.AssertedBy(Requirer)} requires an action for {string.Join("; ", Activities)}, as stated by {StatedBy}";
}
