using RulesKernel.Resolution;
using Srd52Combat.Rules;
using Srd52Combat.Turn;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>communication-cost</c>'s rule reads, beside the <c>gm-requires-action</c> assertion.</summary>
    public sealed partial class CommunicationCostRequest
    {
        /// <summary>What the creature communicates, classified brief or extended by the caller. Required.</summary>
        public CommunicationOnTurn? Communication { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary>
        /// <c>communication-cost</c>: <see cref="TurnRules.Communicates"/>, free or an action, or the
        /// rule's decline where the GM's requirement names the communication and the entry's question
        /// (whether that requirement reaches communication at all) decides the answer.
        /// </summary>
        internal static partial Resolution<object> CommunicationCost(Requests.CommunicationCostRequest request) =>
            Answer(TurnRules.Communicates(
                Demand(request.Communication, request.EntryId, nameof(request.Communication)),
                GmActionRequirementAsserted(request.Assertions)()));
    }
}
