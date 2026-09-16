# 0006 — A mount is a fact the caller states, the gaps the earlier batches hit are closed, and the ruleset is version 5

**Status:** accepted. Follows [0005](0005-the-turn-and-the-round-are-stated-by-the-caller-and-a-group-of-identical-creatures-declines.md),
which took the ruleset to version 5.

## Context

Sixteen entries are built here, in three groups.

- **Mounted combat**, "Combat / Mounted Combat / p. 15" through "Combat / Falling Off / p. 16":
  `mount-eligibility`, `appropriate-anatomy`, `mounting-cost`, `mount-control-requires-training`,
  `controlled-mount-turn`, `independent-mount` and `falling-off`.
- **Underwater combat**, "Combat / Impeded Weapons / p. 16" and "Combat / Fire Resistance / p. 16":
  `underwater-melee`, `underwater-ranged` and `underwater-fire-resistance`.
- **The gaps the earlier batches hit**: `cover-degree`, which `attack-modifiers` declined citing;
  `melee-within-reach` and `single-range`, which `attack-target` declined citing, with `reach`,
  the value `melee-within-reach` depends on; and `move-up-to-speed` and `movement-deduction`, the
  budget `mounting-cost` spends.

Four of the mounted-combat entries carry an unresolved question, and one of them,
`appropriate-anatomy`, is the gap the map split out of `mount-eligibility`: "an appropriate
anatomy" states no measure, no set of values, and nobody who decides. Every one of the rules after
`mount-eligibility` names it in `enabledBy` — directly or through `mounting-cost` — and the backlog
asks each of them for "a state in which `mount-eligibility` or `mounting-cost` holds".

## Decision

**Whether a creature is a mount, and whether anyone is riding it, is a fact the caller states.**
`appropriate-anatomy` can never resolve, so no chain of rules inside this engine ever reaches a
state in which a creature is a mount: `mount-eligibility` answers the refusals it can measure
(unwilling, not at least one size larger) and otherwise declines. The `enabledBy` gate is therefore
a `MountStatement` — `NotAMount`, `ServesAsMount` or `Ridden`, with `StatedBy` — exactly as
`grid-play` is stated for the grid rules (decision 0003, and MAP-FINDINGS finding 2). Where the gate
does not hold, the rule declines `OutsideCurrentScope` citing the gate that is shut:
`mount-eligibility` for a creature nothing states is a mount, `mounting-cost` for a mount nobody is
riding. The engine decides nothing about anatomy by accepting the statement; it records who said the
creature is a mount, and the decline path stays open and tested.

**A rule answers for the corpus's own instances and declines for the rest.**
`mount-control-requires-training` names domesticated horses and mules as having the training, and
"similar creatures" states no measure and names no decider. So the engine answers for a horse and a
mule and declines `RequiresInterpretation` for any other creature, rather than reading "similar" for
the corpus or accepting a caller's "it is trained" where the corpus names nobody who may say so.
`controlled-mount-turn` reads that rule first and declines with it.

**`independent-mount` takes the corpus's own definition as the fact, and declines everything past
it.** An independent mount is "one that lets you ride but ignores your control": whether this mount
ignores the rider's control is stated, and from it the rule answers that the mount retains its place
in the Initiative order. Whether an untrained mount is *always* independent, whether a trained one
may be, and which moves and actions "as it likes" picks, are the entry's open question and the
engine declines each.

**`falling-off` states the DC and the ability and never draws.** The map gives the entry a `draws`
of one d20, and the save is `saving-throws` ("Playing the Game / D20 Tests / p. 6"), `scope: out` —
the same shape as `attack-resolution` (MAP-FINDINGS finding 7). The engine answers what the rule
demands (a DC 10 Dexterity saving throw, naming `saving-throws` as where it is made) and takes the
outcome as a statement. A success keeps the rider on. A failure declines
`RequiresInterpretation`, with what the rule did determine — the rider falls off and lands with the
Prone condition — named in `Attempted`, because which unoccupied space within 5 feet of the mount
the rider lands in, and what happens when there is none, the corpus does not state.

**`cover-degree` answers wherever both readings agree, and declines in the one case they do not.**
The question is whether "that covers at least half of the target" qualifies "Another creature" as
well as "an object". An object is unaffected by it; a creature covering at least half gives Half
Cover under either reading; and a creature never gives more, because the Three-Quarters and Total
rows name only an object (the entry's own note). What is left is a creature covering *less* than
half, which declines `RequiresInterpretation`. A target behind more than one stated source of cover
is `cover-no-stacking`, not built, and declines `UnsupportedRule` citing it.

**`attack-modifiers` and `attack-target` now answer.** Step 2 reads `cover-degree` rather than
deciding Cover itself, and declines with it, for its reason and citing its entry, where it declines.
Step 1 measures a melee attack by `melee-within-reach` (and so by `reach`) and a one-range attack by
`single-range`. Both entries move to `implementedIn` version 6: their behaviour has changed, which
is what that field records.

**The ruleset is `srd-5.2.1-combat` version 6.** Sixteen entries that declined now answer, and two
that declined in part now answer in full, so an engine at version 5 and one at version 6 give
different results for the same inputs. The replay's pinned SHA-256 changes from `c92429e3…dfb2` to
`a8a8105d…7f33`, because the rendered identity line changes from `v5` to `v6`; every other line of
the replay is unchanged. Nothing in this batch draws.

## Consequences

- A replay is comparable only under `srd-5.2.1-combat` v6, PCG32 and map 2.0.0.
- The mounted-combat rules cannot be reached without a caller who states the mount. That is a
  question for the engine's owner (MAP-FINDINGS finding 17): a ruling on "an appropriate anatomy"
  would let `mount-eligibility` answer, and the statement would then be an engine-derived fact
  rather than the caller's.
- `attack-modifiers` has an answering path for the first time, and `attack-target` one for all
  three kinds of attack. What Cover is *worth* is still `cover-bonuses`, whether it counts against
  this attack `cover-origin`, and what Total Cover forbids `total-cover`: none of the three is
  built, and `cover-degree` says only which degree the table gives.
- `mounting-cost` spends `movement-deduction`, which deducts each stated part of a move whole. A
  part the movement left does not cover is not taken, and neither is anything after it.
