namespace Srd52Combat;

/// <summary>
/// The movement and grid handlers' shared demand: a required input whose type is a value type
/// (a Speed, a number of squares, an adjacency), which the caller must set. Nothing is defaulted:
/// a missing input is the caller's error, so it is an <see cref="ArgumentException"/> naming it and
/// not an unresolved result, exactly as for the reference-typed statements.
/// </summary>
internal static partial class Handlers
{
    private static T Demand<T>(T? input, string entryId, string name)
        where T : struct =>
        input ?? throw Missing(entryId, name);
}
