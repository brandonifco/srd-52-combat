# 0002 — Map 2.0.0: tie deciders come from `assertedBy`, a roll with both Advantage and Disadvantage declines, and the ruleset is version 2

**Status:** accepted. Amends [0001](0001-initiative-is-rolled-only-where-the-corpus-fixes-the-draws.md).

## Context

The engine moves from `RulesFactory.Maps.Srd52Combat` 1.0.0 to 2.0.0, produced by rules-factory
0.4.0. For the four implemented entries the map changes in these ways:

- `initiative-ties` now carries `assertedBy: ["GM", "players"]` (rules-factory decision 0025), and
  the generated `MapEntry` and `RegisteredEntry` expose it as `AssertedBy`. Under 1.0.0 the engine
  checked a tie break's decider against its own reading of the rule.
- `initiative-roll` now carries `draws`: one d20 per participant, two for a roll with Advantage or
  Disadvantage, "but one for a roll with both, because they cancel (advantage-disadvantage; …)".
  It also `dependsOn` `incapacitated-condition` (Disadvantage on Initiative) and `invisible-condition`
  (Advantage), both `scope: out`, with `crossReferences` to them and to `initiative-score-option`
  (0026). Before 2.0.0 nothing in the slice gave Advantage to an Initiative roll. Now Invisible
  does, so a roll with both can occur.
- `initiative-ties` and `initiative-ties-uncovered` declare their extraction defect (0024). Neither
  the evidence nor the rule changed.
- `surprised` and `surprise-disadvantage` gain `dependsOn` and `crossReferences` to glossary entries
  (0026). Neither is built, and both still decline `UnsupportedRule`.

The rendered replay names the map version, so the pinned bytes change whatever else is decided.

## Decision

**A tie break's decider is checked against the map.** `TieDeciders.AssertedBy` gives the map's word
for each `TieDecider` ("GM", "players"). `TieDeciders.Named` resolves a party only if the map's
`assertedBy` for `initiative-ties` names it, and `Assign` takes its deciders from `Named`. `BreakTies`
refuses (`ArgumentException`) a tie break whose party the map does not name, or that is not the party
named for that tie. Which party decides which tie (GM for monsters, and for monsters with player
characters; players for characters) is still the engine's reading of p. 13. The map records who may
assert, not how the cases divide. For every input valid under 1.0.0 the behaviour is the same. A
test holds the engine's deciders to exactly the map's `assertedBy`.

**A roll with both Advantage and Disadvantage declines `OutsideCurrentScope`, citing
`advantage-disadvantage` (p. 7), and draws nothing.** The new `D20Mode.AdvantageAndDisadvantage`
lets a caller say so. Without it, the only choices were to misstate the roll or state one of the
two. The map's `draws` does count one d20 here, but row 1 of the correspondence table decides the
case. That Advantage and Disadvantage cancel is `advantage-disadvantage`'s rule, which is
`scope: out`, and `draws.count` is prose that says how many dice a correct engine draws. It does not
bring the cancelling rule into the slice. Advantage alone and Disadvantage alone still decline for
the same reason. So nothing that declined under 1.0.0 now resolves.

**The Incapacitated and Invisible conditions are folded into the caller's `D20Mode`.** The engine
does not ask which conditions a combatant has, just as it does not ask whether one is surprised. The
caller states the roll mode from every source, and any mode but `Straight` declines, citing p. 7.

**The ruleset is `srd-5.2.1-combat` version 2**, and every implemented entry in the overlay says so.
The replay's pinned SHA-256 changes from `0d1918be…2257` to `efbcdbab…acc0`, because the rendered
identity line changes from `v1 … map … 1.0.0` to `v2 … map … 2.0.0`. Every other line of the
replay is unchanged: the same seven d20s, three tie breaks and order. Changing that first line back
reproduces the 1.0.0 hash exactly. The engine now reads a different map and accepts a new roll mode,
and a replay recorded under 1.0.0 names a different map, so the version moves with the bytes rather
than keeping the old version under new bytes.

## Consequences

- A replay is comparable only under `srd-5.2.1-combat` v2, PCG32 and map 2.0.0.
- If a later map adds, removes or renames a party in `initiative-ties`' `assertedBy`, the test on
  `TieDeciders` fails and `TieDeciders.Named` throws. The engine does not quietly keep its own list.
- Building `advantage-disadvantage` would settle all three modes together, and one d20 for a roll
  with both would come with it.
