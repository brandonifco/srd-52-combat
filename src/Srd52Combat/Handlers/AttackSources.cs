using RulesKernel.Resolution;
using Srd52Combat.Attacks;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>attack-sources</c>' rule reads.</summary>
    public sealed partial class AttackSourcesRequest
    {
        /// <summary>What the attacker takes: the Attack action, another action, a Bonus Action, or a Reaction. Required.</summary>
        public AttackSource Source { get; init; }

        /// <summary>Whether the attacker has the Incapacitated condition. Required, never defaulted.</summary>
        public IncapacitatedStatement? Incapacitated { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>attack-sources</c>: <see cref="AttackRules.Sources"/>, that an attack is made, or the rule's decline.</summary>
        internal static partial Resolution<object> AttackSources(Requests.AttackSourcesRequest request) =>
            Answer(AttackRules.Sources(
                request.Source,
                Demand(request.Incapacitated, request.EntryId, nameof(request.Incapacitated))));
    }
}
