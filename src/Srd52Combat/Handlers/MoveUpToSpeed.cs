using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>move-up-to-speed</c>'s rule reads.</summary>
    public sealed partial class MoveUpToSpeedRequest
    {
        /// <summary>The creature's Speed in feet; Speed itself is a parameter (<c>speed-and-size-sources</c>). Required.</summary>
        public int? SpeedInFeet { get; init; }

        /// <summary>The distance asked about, in feet. Required; zero is "you can decide not to move".</summary>
        public int? DistanceFeet { get; init; }

        /// <summary>Who is answerable for the statement of the move. Required, never defaulted.</summary>
        public string? StatedBy { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>move-up-to-speed</c>: <see cref="MovementBudgetRules.UpToSpeed"/>, whether the distance may be moved.</summary>
        internal static partial Resolution<object> MoveUpToSpeed(Requests.MoveUpToSpeedRequest request) =>
            Answer(Resolution<MoveAllowance>.FromValue(MovementBudgetRules.UpToSpeed(
                Demand(request.SpeedInFeet, request.EntryId, nameof(request.SpeedInFeet)),
                Demand(request.DistanceFeet, request.EntryId, nameof(request.DistanceFeet)),
                Demand(request.StatedBy, request.EntryId, nameof(request.StatedBy)))));
    }
}
