using RulesKernel.Resolution;
using Srd52Combat.Initiative;

namespace Srd52Combat;

/// <summary>
/// The hand-written side of the typed contract rules-factory generates in
/// <c>Generated/Contracts.g.cs</c>: one partial method per <c>implemented</c> map entry, each a
/// thin adapter over the rule that implements it.
/// </summary>
/// <remarks>
/// <para>
/// Each file under <c>Handlers/</c> holds one entry's handler and the <c>init</c> properties it
/// reads, declared on the entry's generated request type (the request types are
/// <c>sealed partial</c>, rules-factory#93). A caller resolves an entry through
/// <see cref="EntryPoints"/> with those properties set, and the handler hands them to the rule
/// unchanged. Nothing here decides a rule: every answer, and every decline, is the rule's own.
/// </para>
/// <para>
/// A request built by the dictionary dispatch (<see cref="Registry.Resolve(string, RuleRequest)"/>)
/// has every input at its default. A handler whose rule needs an input it was not given throws
/// <see cref="ArgumentException"/> naming it: a missing input is the caller's error, not a gap in
/// the corpus, so it is not an unresolved result. That includes every statement the caller owes
/// (the Initiative-score option, identical creatures, tie breaks), which are never defaulted.
/// </para>
/// </remarks>
internal static partial class Handlers
{
    private static Resolution<object> Answer<T>(Resolution<T> resolution)
        where T : notnull =>
        resolution.Match(value => Resolution<object>.FromValue(value), Resolution<object>.FromUnresolved);

    private static T Demand<T>(T? input, string entryId, string name)
        where T : class =>
        input ?? throw Missing(entryId, name);

    /// <summary>
    /// The caller's <c>initiative-ties</c> assertion (row 8): demanded when read, so a missing one is
    /// <see cref="AssertionRequiredException"/>, and a value of another type is refused.
    /// </summary>
    private static Func<TieBreaks> TieBreaksAsserted(RuleRequest assertions) =>
        () => assertions.Asserted(MapEntries.InitiativeTies.Id) as TieBreaks
            ?? throw new ArgumentException(
                $"the assertion '{MapEntries.InitiativeTies.Id}' must be a {nameof(TieBreaks)}", nameof(assertions));

    private static ArgumentException Missing(string entryId, string name) =>
        new($"resolving the map entry '{entryId}' needs its request's {name}, and it was not set", name);
}
