using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>movement-deduction</c>'s rule reads.</summary>
    public sealed partial class MovementDeductionRequest
    {
        /// <summary>The Speed the move draws on, in feet. Required.</summary>
        public int? SpeedInFeet { get; init; }

        /// <summary>The parts of the move, in order, as the caller states them. Required, never defaulted; empty is a move of no parts.</summary>
        public IReadOnlyList<MovePart>? Parts { get; init; }

        /// <summary>Who is answerable for the statement of the move. Required, never defaulted.</summary>
        public string? StatedBy { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>movement-deduction</c>: <see cref="MovementBudgetRules.Deduct"/>, what was deducted and what is left.</summary>
        internal static partial Resolution<object> MovementDeduction(Requests.MovementDeductionRequest request) =>
            Answer(Resolution<MovementSpent>.FromValue(MovementBudgetRules.Deduct(
                Demand(request.SpeedInFeet, request.EntryId, nameof(request.SpeedInFeet)),
                Demand(request.Parts, request.EntryId, nameof(request.Parts)),
                Demand(request.StatedBy, request.EntryId, nameof(request.StatedBy)))));
    }
}
