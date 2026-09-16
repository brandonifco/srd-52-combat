using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>group-initiative</c>'s rule reads.</summary>
    public sealed partial class GroupInitiativeRequest
    {
        /// <summary>Every participant, as the caller states them. Required.</summary>
        public IReadOnlyList<Combatant>? Participants { get; init; }

        /// <summary>Who stated the participants and their kinds. Required.</summary>
        public string? StatedBy { get; init; }

        /// <summary>Which participants the caller says form groups of identical creatures. Required, never defaulted.</summary>
        public IdenticalCreaturesStatement? IdenticalCreatures { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary>
        /// <c>group-initiative</c>: <see cref="GroupInitiativeRules.Roll"/>, who the GM rolls for, or
        /// the rule's decline where a group of identical creatures is stated and how many rolls it
        /// takes turns on the entry's unresolved question.
        /// </summary>
        internal static partial Resolution<object> GroupInitiative(Requests.GroupInitiativeRequest request) =>
            Answer(GroupInitiativeRules.Roll(
                Demand(request.Participants, request.EntryId, nameof(request.Participants)),
                Demand(request.StatedBy, request.EntryId, nameof(request.StatedBy)),
                Demand(request.IdenticalCreatures, request.EntryId, nameof(request.IdenticalCreatures))));
    }
}
