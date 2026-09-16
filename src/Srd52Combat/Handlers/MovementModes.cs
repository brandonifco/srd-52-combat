using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>movement-modes</c>' rule reads.</summary>
    public sealed partial class MovementModesRequest
    {
        /// <summary>The modes the move is made of, in order. Required.</summary>
        public IReadOnlyList<MovementMode>? Modes { get; init; }

        /// <summary>Who stated the move. Required.</summary>
        public string? StatedBy { get; init; }

        /// <summary>A mode whose cost is asked for; the costs are in the Rules Glossary, outside the extent. Optional.</summary>
        public MovementMode? AskingWhatAModeCosts { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>movement-modes</c>: <see cref="MovementRules.Combine"/>, the one move, or the rule's decline.</summary>
        internal static partial Resolution<object> MovementModes(Requests.MovementModesRequest request) =>
            Answer(MovementRules.Combine(
                Demand(request.Modes, request.EntryId, nameof(request.Modes)),
                Demand(request.StatedBy, request.EntryId, nameof(request.StatedBy)),
                request.AskingWhatAModeCosts));
    }
}
