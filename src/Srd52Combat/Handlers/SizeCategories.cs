using RulesKernel.Resolution;
using Srd52Combat.Movement;
using Srd52Combat.Rules;

namespace Srd52Combat.Requests
{
    /// <summary>The inputs <c>size-categories</c>' rule reads: none. The entry states a value.</summary>
    public sealed partial class SizeCategoriesRequest
    {
    }
}

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary><c>size-categories</c>: <see cref="MovementRules.Sizes"/>, the categories from smallest to largest.</summary>
        internal static partial Resolution<object> SizeCategories(Requests.SizeCategoriesRequest request) =>
            Answer(Resolution<SizeOrder>.FromValue(MovementRules.Sizes()));
    }
}
