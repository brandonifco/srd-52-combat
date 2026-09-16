using RulesKernel.Resolution;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>appropriate-anatomy</c>'s rule reads.</summary>
    public sealed partial class AppropriateAnatomyRequest
    {
        /// <summary>The creature asked about. Required.</summary>
        public string? Candidate { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>appropriate-anatomy</c>: <see cref="MountRules.AppropriateAnatomy"/>, the gap's decline.</summary>
        internal static partial Resolution<object> AppropriateAnatomy(Requests.AppropriateAnatomyRequest request) =>
            MountRules.AppropriateAnatomy(Demand(request.Candidate, request.EntryId, nameof(request.Candidate)));
    }
}
