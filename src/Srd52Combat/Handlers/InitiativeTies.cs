using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>initiative-ties</c>' rule reads, beside the assertion itself (a <see cref="TieBreaks"/>).</summary>
    public sealed partial class InitiativeTiesRequest
    {
        /// <summary>Every combatant's Initiative, which fixes the ties the assertion must break. Required.</summary>
        public IReadOnlyList<InitiativeCount>? Counts { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary>
        /// <c>initiative-ties</c>, an assertion (row 8): the caller's tie breaks, checked by
        /// <see cref="InitiativeRules.BreakTies"/> against the ties and the deciders the rule names,
        /// and never inferred. It always answers, so the row's default (the raw value) never does.
        /// </summary>
        static partial void InitiativeTies(Requests.InitiativeTiesRequest request, ref Resolution<object>? resolution) =>
            resolution = Answer(InitiativeRules.BreakTies(
                Demand(request.Counts, request.EntryId, nameof(request.Counts)),
                TieBreaksAsserted(request.Assertions)));
    }
}
