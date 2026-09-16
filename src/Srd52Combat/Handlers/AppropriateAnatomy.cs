using RulesKernel.Resolution;
using Srd52Combat.Mounts;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>appropriate-anatomy</c>'s rule reads.</summary>
    public sealed partial class AppropriateAnatomyRequest
    {
        /// <summary>
        /// What the GM determined about the creature's anatomy, and about which creature. Required,
        /// never defaulted: under the owner's ruling of 2026-09-16 the anatomy is the GM's call, and
        /// the engine never assumes one.
        /// </summary>
        public AnatomyStatement? Anatomy { get; init; }
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary>
        /// <c>appropriate-anatomy</c>: <see cref="MountRules.AppropriateAnatomy"/>, the GM's
        /// determination naming the owner's ruling, or the decline where nothing was determined.
        /// </summary>
        internal static partial Resolution<object> AppropriateAnatomy(Requests.AppropriateAnatomyRequest request) =>
            Answer(MountRules.AppropriateAnatomy(Demand(request.Anatomy, request.EntryId, nameof(request.Anatomy))));
    }
}
