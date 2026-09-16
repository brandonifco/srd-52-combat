using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Cover;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>attack-modifiers</c>' rule reads.</summary>
    public sealed partial class AttackModifiersRequest
    {
        /// <summary>What the caller states lies between attacker and target (<c>cover-degree</c>). Required, never defaulted.</summary>
        public IReadOnlyList<CoveringObstacle>? Obstacles { get; init; }

        /// <summary>What the rules of this slice gave the attack roll, as the caller resolved them. Required, never defaulted.</summary>
        public IReadOnlyList<RollDetermination>? Determinations { get; init; }

        /// <summary>Penalties and bonuses from spells, special abilities and other effects, each of which states its own. Required, never defaulted.</summary>
        public IReadOnlyList<string>? Other { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>attack-modifiers</c>: <see cref="AttackRules.Modifiers"/>, the step's determinations, or the decline <c>cover-degree</c> gave.</summary>
        internal static partial Resolution<object> AttackModifiers(Requests.AttackModifiersRequest request) =>
            Answer(AttackRules.Modifiers(
                Demand(request.Obstacles, request.EntryId, nameof(request.Obstacles)),
                Demand(request.Determinations, request.EntryId, nameof(request.Determinations)),
                Demand(request.Other, request.EntryId, nameof(request.Other))));
    }
}
