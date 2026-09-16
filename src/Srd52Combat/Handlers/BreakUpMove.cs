using RulesKernel.Resolution;
using Srd52Combat.Rules;
using Srd52Combat.Turn;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>break-up-move</c>'s rule reads.</summary>
    public sealed partial class BreakUpMoveRequest
    {
        /// <summary>The creature's Speed, as the caller states it. Required.</summary>
        public int? Speed { get; init; }

        /// <summary>The turn's steps, in the order the creature takes them: the move's parts and what they are broken around. Required.</summary>
        public IReadOnlyList<TurnStep>? Steps { get; init; }

        /// <summary>Who is answerable for the statement of the turn. Required.</summary>
        public string? StatedBy { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>break-up-move</c>: <see cref="TurnRules.BreakUp"/>, each part of the move and the movement left after it.</summary>
        internal static partial Resolution<object> BreakUpMove(Requests.BreakUpMoveRequest request) =>
            Answer(TurnRules.BreakUp(
                Demand(request.Speed, request.EntryId, nameof(request.Speed)),
                Demand(request.Steps, request.EntryId, nameof(request.Steps)),
                Demand(request.StatedBy, request.EntryId, nameof(request.StatedBy))));
    }
}
