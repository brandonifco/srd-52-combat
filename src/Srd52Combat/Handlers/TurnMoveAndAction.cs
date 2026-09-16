using RulesKernel.Resolution;
using Srd52Combat.Rules;
using Srd52Combat.Turn;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>turn-move-and-action</c>'s rule reads.</summary>
    public sealed partial class TurnMoveAndActionRequest
    {
        /// <summary>The creature's Speed, as the caller states it. Required.</summary>
        public int? Speed { get; init; }

        /// <summary>The turn's steps, in the order the creature takes them. Required; empty is a turn that does nothing.</summary>
        public IReadOnlyList<TurnStep>? Steps { get; init; }

        /// <summary>Who is answerable for the statement of the turn. Required.</summary>
        public string? StatedBy { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>turn-move-and-action</c>: <see cref="TurnRules.Take"/>, the turn's budget as the creature spent it.</summary>
        internal static partial Resolution<object> TurnMoveAndAction(Requests.TurnMoveAndActionRequest request) =>
            Answer(TurnRules.Take(
                Demand(request.Speed, request.EntryId, nameof(request.Speed)),
                Demand(request.Steps, request.EntryId, nameof(request.Steps)),
                Demand(request.StatedBy, request.EntryId, nameof(request.StatedBy))));
    }
}
