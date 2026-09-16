using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>creature-space-difficult-terrain</c>' rule reads.</summary>
    public sealed partial class CreatureSpaceDifficultTerrainRequest
    {
        /// <summary>The creature whose space it is, as the caller states it: its size and whether it is your ally. Required, never defaulted.</summary>
        public CreatureInSpace? Other { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>creature-space-difficult-terrain</c>: <see cref="MovementRules.SpaceOf"/>, whether its space is Difficult Terrain for you.</summary>
        internal static partial Resolution<object> CreatureSpaceDifficultTerrain(Requests.CreatureSpaceDifficultTerrainRequest request) =>
            Answer(Resolution<CreatureSpaceTerrain>.FromValue(
                MovementRules.SpaceOf(Demand(request.Other, request.EntryId, nameof(request.Other)))));
    }
}
