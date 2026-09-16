using RulesKernel.Identity;

namespace Srd52Combat;

/// <summary>The engine's ruleset.</summary>
public static class Ruleset
{
    /// <summary>
    /// The engine's replay identity: the ruleset, the replay schema, the pinned corpus, and the
    /// random algorithm. The corpus declares <c>randomness: seeded</c> (rules-factory decision 0019),
    /// and every draw goes through RulesKernel.Randomness's PCG32, so a combat replays from its seed
    /// only under this identity.
    /// </summary>
    public static ReplayCompatibilityIdentity Identity { get; } = new(
        ruleset: new RulesetVersion("srd-5.2.1-combat", 5),
        replaySchema: new ReplaySchemaVersion(1),
        sourceBaselines: [MapEntries.Baseline],
        randomAlgorithm: RandomAlgorithmId.Pcg32SetSeq64XshRr32);
}
