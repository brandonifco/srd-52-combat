using RulesKernel.Randomness;
using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>initiative-roll</c>'s rule reads.</summary>
    public sealed partial class InitiativeRollRequest
    {
        /// <summary>Every participant, in the order their d20s are drawn. Required.</summary>
        public IReadOnlyList<Combatant>? Participants { get; init; }

        /// <summary>Who stated the participants: their kinds, modifiers and roll modes. Required.</summary>
        public string? StatedBy { get; init; }

        /// <summary>Whether the GM uses Initiative scores instead of rolling. Required, never defaulted.</summary>
        public InitiativeScoreOptionStatement? ScoreOption { get; init; }

        /// <summary>Which participants form groups of identical creatures. Required, never defaulted.</summary>
        public IdenticalCreaturesStatement? IdenticalCreatures { get; init; }

        /// <summary>The seeded generator the d20s are drawn from. Required.</summary>
        public IRandomSource? Source { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>initiative-roll</c>: <see cref="InitiativeRules.Roll"/>, the rolls, or the rule's decline.</summary>
        internal static partial Resolution<object> InitiativeRoll(Requests.InitiativeRollRequest request) =>
            Answer(InitiativeRules.Roll(
                Demand(request.Participants, request.EntryId, nameof(request.Participants)),
                Demand(request.StatedBy, request.EntryId, nameof(request.StatedBy)),
                Demand(request.ScoreOption, request.EntryId, nameof(request.ScoreOption)),
                Demand(request.IdenticalCreatures, request.EntryId, nameof(request.IdenticalCreatures)),
                Demand(request.Source, request.EntryId, nameof(request.Source))));
    }
}
