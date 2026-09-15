using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>initiative-ties-uncovered</c>' rule reads.</summary>
    public sealed partial class InitiativeTiesUncoveredRequest
    {
        /// <summary>Every combatant's Initiative. Required.</summary>
        public IReadOnlyList<InitiativeCount>? Counts { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>initiative-ties-uncovered</c>: <see cref="InitiativeRules.Assign"/>, who decides each tie, or the decline where the rule names nobody.</summary>
        internal static partial Resolution<object> InitiativeTiesUncovered(Requests.InitiativeTiesUncoveredRequest request) =>
            Answer(InitiativeRules.Assign(Demand(request.Counts, request.EntryId, nameof(request.Counts))));
    }
}
