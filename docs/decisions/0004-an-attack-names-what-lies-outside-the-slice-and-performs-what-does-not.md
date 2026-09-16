# 0004 — An attack names what lies outside the slice and performs what does not, and the ruleset is version 4

**Status:** accepted. Follows [0003](0003-movement-and-space-the-grid-is-a-variant-the-caller-opts-into.md), which took the ruleset to version 3.

## Context

Thirteen entries of "Combat / Making an Attack" (pp. 14–15), its "Unseen Attackers and Targets"
sidebar, "Range", "Ranged Attacks in Close Combat" and "Opportunity Attacks" are built here:
`attack-sources`, `attack-structure`, `attack-target`, `attack-modifiers`, `attack-resolution`,
`unseen-attacker-advantage`, `unseen-target-disadvantage`, `wrong-location-misses`,
`hidden-attacker-revealed`, `normal-and-long-range`, `ranged-in-close-combat`,
`opportunity-attack` and `opportunity-attack-avoidance`.

Every one of them leans on something the engine does not hold. The d20 of an attack roll is
`attack-rolls` (p. 7) and its damage is `damage-rolls` (p. 16); how Advantage and Disadvantage
resolve, and what happens when both apply, is `advantage-disadvantage` (p. 7); the Actions table,
Bonus Actions, Reactions, the Incapacitated condition, Disengage and the Rules Glossary's
Opportunity Attacks entry are all `scope: out`. Two entries this slice depends on are in the map
and not built: `cover-degree` and `melee-within-reach`, and with them `single-range`.

## Decision

**A fact the corpus leaves to the table is a statement the caller makes, attributed and recorded,
never inferred.** Whether the attacker can see the target, whether the target can see the attacker,
whether a creature is hidden or Incapacitated, which enemies stand within 5 feet, whether the attack
roll hit, whether the attack's own rules specify otherwise, how a creature left the attacker's
reach: each is a record in `Srd52Combat.Attacks` carrying `StatedBy`, demanded by the handler and
refused (`ArgumentException`) when left out. This follows `initiative-roll`'s Initiative-score and
identical-creatures statements, and rules-factory decisions 0021 and 0025.

**A rule outside the slice declines `OutsideCurrentScope` citing its entry; a rule in the map that
this engine has not built declines `UnsupportedRule` citing its entry.** So `attack-sources`
declines for any source but the Attack action, citing `actions-table`, `bonus-actions` or
`reactions`; `attack-target` declines a melee attack citing `melee-within-reach` and a one-range
ranged attack citing `single-range`; `opportunity-attack` declines for an Incapacitated attacker,
for a spent Reaction, and for a creature that leaves reach by none of the means the glossary lists.

**Advantage and Disadvantage are named, never combined.** Each of `unseen-attacker-advantage`,
`unseen-target-disadvantage`, `ranged-in-close-combat` and `normal-and-long-range` answers with its
own `RollEffect` and the citation for it. Nothing here adds two of them together or cancels them:
that is `advantage-disadvantage`'s rule, `scope: out`, as decision 0002 already held for Initiative.

**`attack-structure` is the order of the three steps, and nothing more.** Its `note` says so ("The
order of the three steps … Each step is its own entry"). It answers with the three steps, each
naming the entry that holds it, and does not run them: a caller resolves `attack-target`,
`attack-modifiers` and `attack-resolution` in turn with the inputs each needs. So the entry takes
no input and cannot fail.

**`attack-resolution` says whether damage is rolled, and names the rule that rolls it.** The roll
and the damage are both outside the slice; what the sentence adds is that damage is rolled on a hit,
and only on a hit, unless the particular attack's own rules specify otherwise. Given the outcome the
caller states, the engine answers that — on a hit naming `damage-rolls` (p. 16) as the authority for
the roll the caller must then make. The map's `draws` for this entry names a d20 and damage dice;
the engine draws neither, because both drawing rules are `scope: out`. Nothing in this batch is
seeded, and the replay identity is unchanged in kind.

**`attack-modifiers` declines `UnsupportedRule` citing `cover-degree`.** The step determines three
things. Two of them the engine can do: the Advantage and Disadvantage the rules of this slice give,
which the caller resolves and hands in, and the penalties and bonuses of spells, special abilities
and other effects, which each state their own and the caller states here. The third, whether the
target has Cover, is `cover-degree`, an entry the map has and this engine has not built. So the step
cannot be completed, and it declines citing exactly what is missing, with everything it did
determine named in the decline's `Attempted`. This is the same shape as `initiative-roll`'s decline
citing `group-initiative`. The entry is `implemented` rather than `blocked` because the engine does
perform the step and reports precisely where it stops; building `cover-degree` turns the decline
into an answer without changing anything else.

**The ruleset is `srd-5.2.1-combat` version 4.** Thirteen entries that declined now answer, so an
engine at version 3 and one at version 4 give different results for the same inputs. The replay's
pinned SHA-256 changes from `df5c2b91…3d35` to `20fb2e9b…88f9`, because the rendered identity line changes from `v3` to `v4`; every
other line of the replay is unchanged. The Initiative and movement entries keep `implementedIn`
versions 2 and 3 in the overlay: that field records the version their implementation belongs to,
and neither their code nor their behaviour has changed.

## Consequences

- A replay is comparable only under `srd-5.2.1-combat` v4, PCG32 and map 2.0.0.
- `attack-modifiers` has no answering path until `cover-degree` is built, and `attack-target` none
  for melee or one-range attacks until `melee-within-reach` and `single-range` are. Each says so in
  its decline.
- Building `attack-rolls`, `damage-rolls` or `advantage-disadvantage` — all `scope: out` today —
  would let `attack-resolution` roll rather than name, and would let a caller combine the
  determinations the four modifier entries give.
