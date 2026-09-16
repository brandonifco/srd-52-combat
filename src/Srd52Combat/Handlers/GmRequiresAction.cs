using RulesKernel.Resolution;
using Srd52Combat.Rules;
using Srd52Combat.Turn;

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary>
        /// The caller's <c>gm-requires-action</c> assertion (row 8): demanded when read, so a missing
        /// one is <see cref="AssertionRequiredException"/>, and a value of another type is refused.
        /// </summary>
        /// <param name="assertions">What the caller asserts.</param>
        /// <returns>Reads the statement.</returns>
        private static Func<GmActionRequirement> GmActionRequirementAsserted(RuleRequest assertions) =>
            () => assertions.Asserted(MapEntries.GmRequiresAction.Id) as GmActionRequirement
                ?? throw new ArgumentException(
                    $"the assertion '{MapEntries.GmRequiresAction.Id}' must be a {nameof(GmActionRequirement)}", nameof(assertions));

        /// <summary>
        /// <c>gm-requires-action</c>, an assertion (row 8): the GM's own determination that an
        /// activity requires an action, checked by <see cref="TurnRules.Requires"/> to be made by a
        /// party the map's <c>assertedBy</c> names, and never inferred. It always answers, so the
        /// row's default (the raw value) never does.
        /// </summary>
        static partial void GmRequiresAction(Requests.GmRequiresActionRequest request, ref Resolution<object>? resolution) =>
            resolution = Answer(TurnRules.Requires(GmActionRequirementAsserted(request.Assertions)()));
    }
}
