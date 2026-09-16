using RulesKernel.Resolution;
using Srd52Combat.Initiative;
using Srd52Combat.Rules;
using Srd52Combat.Turn;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>combat-steps</c>' rule reads.</summary>
    public sealed partial class CombatStepsRequest
    {
        /// <summary>Step 1: where all the characters and monsters are, as the GM states them. Required.</summary>
        public PositionsStatement? Positions { get; init; }

        /// <summary>
        /// Step 2's result: the Initiative order (<c>initiative-order</c>). Required, and never
        /// defaulted: step 3's turns are taken in this order, so no turn is taken before Initiative
        /// is rolled.
        /// </summary>
        public TurnOrder? Order { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>combat-steps</c>: <see cref="RoundRules.Steps"/>, the three steps in the corpus's order.</summary>
        internal static partial Resolution<object> CombatSteps(Requests.CombatStepsRequest request) =>
            Answer(RoundRules.Steps(
                Demand(request.Positions, request.EntryId, nameof(request.Positions)),
                Demand(request.Order, request.EntryId, nameof(request.Order))));
    }
}
