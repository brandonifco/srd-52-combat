using RulesKernel.Resolution;
using Srd52Combat.Rules;
using Srd52Combat.Turn;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>doing-nothing</c>'s rule reads.</summary>
    public sealed partial class DoingNothingRequest
    {
        /// <summary>The creature's Speed, as the caller states it. Required.</summary>
        public int? Speed { get; init; }

        /// <summary>What the creature does on its turn, in order. Required; empty is forgoing everything.</summary>
        public IReadOnlyList<TurnStep>? Steps { get; init; }

        /// <summary>Who is answerable for the statement of the turn. Required.</summary>
        public string? StatedBy { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>doing-nothing</c>: <see cref="TurnRules.Forgo"/>, what the turn forgoes, which the rule permits.</summary>
        internal static partial Resolution<object> DoingNothing(Requests.DoingNothingRequest request) =>
            Answer(TurnRules.Forgo(
                Demand(request.Speed, request.EntryId, nameof(request.Speed)),
                Demand(request.Steps, request.EntryId, nameof(request.Steps)),
                Demand(request.StatedBy, request.EntryId, nameof(request.StatedBy))));
    }
}
