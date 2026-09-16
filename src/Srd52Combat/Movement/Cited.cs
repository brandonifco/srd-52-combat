namespace Srd52Combat.Movement;

/// <summary>
/// How a movement or grid decline names what was attempted. Eight of these entries share three
/// citations ("Combat / Moving around Other Creatures / p. 14", "Combat / Playing on a Grid /
/// p. 13", "Combat / Movement and Position / p. 14"), so the entry id is what tells two declines
/// from the same page apart.
/// </summary>
internal static class Cited
{
    internal static string Attempting(MapEntry entry) =>
        $"resolve the map entry '{entry.Id}' [{entry.Locator.Citation}]";
}
