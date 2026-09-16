using RulesKernel.Resolution;

namespace Srd52Combat.Rules;

/// <summary>
/// How the cover, targeting, movement-budget, mounted-combat and underwater rules name what they
/// were asked and what they declined: the same shape <see cref="AttackRules.Attempting"/> and
/// <see cref="Srd52Combat.Movement.Cited"/> give the earlier batches. Several of these entries share
/// a citation ("Combat / Controlling a Mount / p. 16", "Combat / Impeded Weapons / p. 16"), so the
/// entry id in <see cref="UnresolvedResult.Attempted"/> is what tells two declines from the same
/// page apart.
/// </summary>
internal static class Declines
{
    internal static string Attempting(MapEntry entry) =>
        $"resolve the map entry '{entry.Id}' [{entry.Locator.Citation}]";

    internal static Resolution<T> Of<T>(UnresolvedReason reason, string attempted, MapEntry cited)
        where T : notnull =>
        Resolution<T>.FromUnresolved(new UnresolvedResult(reason, attempted, cited.Locator));
}
