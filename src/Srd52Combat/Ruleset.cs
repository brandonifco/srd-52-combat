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
    /// <remarks>
    /// Version 7 is Brandon's rulings of 2026-09-15 and 2026-09-16 (<c>docs/decisions/0007</c>): six
    /// answers changed, one of them a roll. Schema 2 is the <c>rulings</c> line a recorded combat now
    /// carries, which is the rulings its answers relied on (rules-factory decision 0027 § 4).
    /// </remarks>
    public static ReplayCompatibilityIdentity Identity { get; } = new(
        ruleset: new RulesetVersion("srd-5.2.1-combat", 7),
        replaySchema: new ReplaySchemaVersion(2),
        sourceBaselines: [MapEntries.Baseline],
        randomAlgorithm: RandomAlgorithmId.Pcg32SetSeq64XshRr32);
}
