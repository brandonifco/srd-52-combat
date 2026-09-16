using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>ending-turn-in-occupied-space</c>' rule reads.</summary>
    public sealed partial class EndingTurnInOccupiedSpaceRequest
    {
        /// <summary>Your size category, as the caller states it. Required, never inferred.</summary>
        public CreatureSize? YourSize { get; init; }

        /// <summary>How the turn ended: whether another creature was in your space as it ended. Required, never defaulted.</summary>
        public EndOfTurnStatement? Turn { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>ending-turn-in-occupied-space</c>: <see cref="MovementRules.EndTurn"/>, whether you have the Prone condition.</summary>
        internal static partial Resolution<object> EndingTurnInOccupiedSpace(Requests.EndingTurnInOccupiedSpaceRequest request) =>
            Answer(Resolution<EndOfTurnRuling>.FromValue(MovementRules.EndTurn(
                Demand(request.YourSize, request.EntryId, nameof(request.YourSize)),
                Demand(request.Turn, request.EntryId, nameof(request.Turn)))));
    }
}
