using RulesKernel.Resolution;
using Srd52Combat.Rules;

namespace Srd52Combat
{
    internal static partial class Handlers
    {
        /// <summary>
        /// <c>attack-structure</c>: <see cref="AttackRules.Structure"/>, the three steps in order.
        /// The entry is the order and nothing else, so its request carries no inputs; each step is
        /// its own entry, which the caller resolves in turn.
        /// </summary>
        internal static partial Resolution<object> AttackStructure(Requests.AttackStructureRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            return Answer(AttackRules.Structure());
        }
    }
}
