using RulesKernel.Resolution;
using Srd52Combat.Rules;
using Srd52Combat.Underwater;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>underwater-fire-resistance</c>'s rule reads.</summary>
    public sealed partial class UnderwaterFireResistanceRequest
    {
        /// <summary>Whether the thing is underwater. Required, never defaulted.</summary>
        public UnderwaterStatement? Where { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>underwater-fire-resistance</c>: <see cref="UnderwaterRules.Fire"/>, the Resistance to Fire damage.</summary>
        internal static partial Resolution<object> UnderwaterFireResistance(Requests.UnderwaterFireResistanceRequest request) =>
            Answer(UnderwaterRules.Fire(Demand(request.Where, request.EntryId, nameof(request.Where))));
    }
}
