using RulesKernel.Resolution;
using Srd52Combat.Mounts;
using Srd52Combat.Movement;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>mount-eligibility</c>'s rule reads.</summary>
    public sealed partial class MountEligibilityRequest
    {
        /// <summary>The rider's size category (<c>size-categories</c>). Required.</summary>
        public CreatureSize? Rider { get; init; }

        /// <summary>The candidate's name or handle. Required.</summary>
        public string? Candidate { get; init; }

        /// <summary>The candidate's size category. Required.</summary>
        public CreatureSize? CandidateSize { get; init; }

        /// <summary>Whether the candidate is willing. Required, never defaulted.</summary>
        public WillingStatement? Willing { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>mount-eligibility</c>: <see cref="MountRules.Eligible"/>, the refusal, or the decline citing <c>appropriate-anatomy</c>.</summary>
        internal static partial Resolution<object> MountEligibility(Requests.MountEligibilityRequest request) =>
            Answer(MountRules.Eligible(
                Demand(request.Rider, request.EntryId, nameof(request.Rider)),
                Demand(request.Candidate, request.EntryId, nameof(request.Candidate)),
                Demand(request.CandidateSize, request.EntryId, nameof(request.CandidateSize)),
                Demand(request.Willing, request.EntryId, nameof(request.Willing))));
    }
}
