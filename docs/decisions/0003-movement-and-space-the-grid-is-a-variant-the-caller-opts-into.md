# 0003 — Movement and space: the grid is a variant the caller opts into, and the values it rests on are built with it

**Status:** accepted. Extends [0001](0001-initiative-is-rolled-only-where-the-corpus-fixes-the-draws.md) and
[0002](0002-map-2-0-0-deciders-from-assertedby-and-both-advantage-and-disadvantage-declines.md).

## Context

The second batch built is movement and space: `movement-modes`, `dropping-prone`,
`moving-through-creatures`, `creature-space-difficult-terrain`,
`no-willing-end-in-occupied-space`, `ending-turn-in-occupied-space`, and the grid variant's
`grid-speed-in-squares`, `grid-entering-square` and `grid-corners`. Three questions had to be
settled before any of them could be written.

**The grid rules are reached only from `grid-play`.** Every grid entry names `grid-play` in
`enabledBy`: "If you play using a square grid and miniatures or other tokens, follow these rules."
Whether the table uses a grid is a caller-supplied parameter and gets no entry of its own (the
map's note on `grid-play`, rules-factory decision 0010), so nothing in the engine can know it.

**Four of the entries rest on a value the engine had not built.** `grid-speed-in-squares` and
`grid-entering-square` `dependsOn` `grid-square-size` ("Each square represents 5 feet"), and
`moving-through-creatures` and `ending-turn-in-occupied-space` `dependsOn` `size-categories` (the
categories "from smallest (Tiny) to largest (Gargantuan)"). Both are `kind: value`, `scope: in`
and were `mapped`, which is correspondence row 5: an operation with an unimplemented value
dependency answers `MissingRulesData`. An engine cannot divide a Speed by a square it has not
built, or count "two sizes larger" along an order it does not hold.

**Two of the entries are `ambiguity.fate: unresolved`.** `moving-through-creatures` asks whether
"two sizes larger or smaller" means exactly two or two or more, and whether the four kinds of space
the sentence permits are the only ones. `grid-entering-square` asks what a square occupied by a
creature whose space is not Difficult Terrain for you costs. Nobody has ruled on either.

## Decision

**Every grid rule demands the caller's `GridPlayStatement`, and off the grid declines
`OutsideCurrentScope` citing `grid-play`.** The grid is a variant the table opts into, so the
engine never assumes one, and never infers it from the shape of a question (a Speed in squares
asked for is not a grid). Off the grid the default rules govern and distances are in feet, and no
grid entry answers. `OutsideCurrentScope` is the reason `faa-part-107` gives for a rule a stated
waiver suspends: the rule exists and is not in force here. The decline cites `grid-play`, the rule
that would have put it in force, not the entry that declined.

**`grid-square-size` and `size-categories` are built in this batch**, though neither was in it.
They are the values row 5 names, they are `scope: in`, and the four entries above cannot be
answered without them: building the dependants and leaving the values `mapped` would have put the
engine's answers where the map says `MissingRulesData`. Each gets its own entry point, tests and
overlay record, and closes its own backlog issue. `creature-size-space` (the table's spaces in feet
and squares) is *not* built: nothing in this batch reads it.

**The unresolved questions decline, and the cases the corpus settles do not.**
`moving-through-creatures` answers "you may pass through" for the four kinds of space the sentence
names, counting a creature *exactly* two sizes away, on which both readings agree; every other
creature — more than two sizes away, or fewer and none of the other cases — declines
`RequiresInterpretation` citing the entry. `grid-entering-square` prices an unoccupied square at 1,
a square of Difficult Terrain at 2, and a square held by a creature that is neither Tiny nor your
ally at 2 (its space is Difficult Terrain, `creature-space-difficult-terrain`); a square held by a
Tiny creature or an ally, whose terrain is not otherwise difficult, declines.

**A fact the rule tests is the caller's statement, never the engine's inference** (rules-factory
decision 0025): a creature's size category, whether it is your ally, whether it has the
Incapacitated condition, whether a square holds Difficult Terrain, whether a terrain feature fills
its space, whether a move was willing, and whether another creature was in your space as your turn
ended. Each is demanded, attributed to whoever stated it, and recorded on the answer. A missing one
is an `ArgumentException` naming it, not an unresolved result.

**The ruleset is `srd-5.2.1-combat` version 3**, and every entry this batch implements says so in
the overlay. The Initiative entries keep version 2: nothing about a roll or an order changed. The
replay's pinned SHA-256 changes from `efbcdbab…acc0` to `df5c2b91…3d35` because the rendered identity line now reads `v3`; every other line of the replay
is unchanged.

## Consequences

- A replay is comparable only under `srd-5.2.1-combat` v3, PCG32 and map 2.0.0.
- Building `grid-play` itself will not change these rules' behaviour: the statement the caller
  makes is what the entry describes, and the rule that entry states ("follow these rules") is what
  the gate already does.
- Building `difficult-terrain` will not change `grid-entering-square` either: which squares hold
  Difficult Terrain stays the caller's statement, because that entry's own question is unresolved.
- A ruling on either unresolved question would turn a decline into an answer, and would be a
  ruleset version of its own (rules-factory decision 0027).
