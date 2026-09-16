# Where the map turned out to be wrong

A running log, kept while building this engine from `RulesFactory.Maps.Srd52Combat`. One section
per finding: what the map says, what implementing it revealed, and whether the map or the code is
at fault. Quotations of SRD 5.2.1 here are under the corpus's licence, CC-BY-4.0; see `NOTICE`.

Counts so far: **6 findings from the movement and space batch (map 2.0.0, ruleset version 3). None
is a fault in the map.** Four are records of something the map is right about and that a reader of
the engine would otherwise have to rediscover (1, 3, 4, 5); one is a gap in the method rather than
in the map (2); one is a question for the engine's owner (6).

---

## 1. Four entries in one batch rested on two `kind: value` entries in none

`grid-speed-in-squares` and `grid-entering-square` `dependsOn` `grid-square-size`;
`moving-through-creatures` and `ending-turn-in-occupied-space` `dependsOn` `size-categories`. Both
targets are `kind: value`, `scope: in`, and were `status: mapped`, which is correspondence row 5:
the dependants answer `MissingRulesData`. The map is right — an engine cannot divide a Speed by a
square it has not built, nor count "two sizes larger" along an order it does not hold — and the
`dependsOn` edges say exactly that. What the build found is that the row is invisible in the
backlog: a backlog item's page lists its `dependsOn` but does not say that implementing it while a
`kind: value` dependency is `mapped` puts the entry on row 5. Both values were built in this batch
([decision 0003](docs/decisions/0003-movement-and-space-the-grid-is-a-variant-the-caller-opts-into.md)).

## 2. Nothing in the method says what an entry does when its `enabledBy` does not hold

Every grid entry names `grid-play` in `enabledBy`, and `grid-play`'s note says whether the table
uses a grid is a caller-supplied parameter with no entry of its own. So the engine must be told,
and must answer something when the answer is "no grid". The map fixes the runtime reason for a
`scope: out` rule, an unbuilt one, an unresolved question and an assertion (the correspondence
table), but not for a rule whose gate is open and whose gate does not hold. This engine declines
`OutsideCurrentScope` citing `grid-play`, by analogy with `faa-part-107`'s suspension by a stated
waiver. A resolved "the rule does not apply" would have been defensible too. The map is not wrong;
the method has no field for it.

## 3. "Two sizes smaller" adds nothing for a Medium creature

`moving-through-creatures` lets you pass through the space of "a Tiny creature, or a creature that
is two sizes larger or smaller than you". Two sizes smaller than Medium is Tiny, which the previous
limb already covers, so the smaller half of the limb first does any work for a Large creature (two
sizes smaller is Small). It is worth knowing when reading the tests, which demonstrate the smaller
half with a Large creature and a Small one.

## 4. An Incapacitated creature's space can be passed through and is still Difficult Terrain

`moving-through-creatures` names four kinds of space you can pass through, the Incapacitated
creature's among them. `creature-space-difficult-terrain` exempts two kinds from Difficult Terrain,
Tiny and your ally, and the Incapacitated creature is not one of them. The two rules are consistent
and the engine implements both literally: an Incapacitated stranger's space is passable and costs
double. Nothing in the map hides this; nothing in the map points it out either.

## 5. You may pass through an ally's space and may not end a move there

`no-willing-end-in-occupied-space` says "another creature", with no exception for an ally or a Tiny
creature, though both are exceptions in the two neighbouring rules. The engine implements it as
written. Same observation as finding 4: the asymmetry is the corpus's, not the map's.

## 6. `dropping-prone`'s "On your turn" has no carrier

"On your turn, you can give yourself the Prone condition …". Whose turn it is belongs to
`turn-move-and-action`, which is not built, and the map gives `dropping-prone` no `enabledBy` or
parameter for it. The engine answers the permission the rule grants and takes only the Speed; it
does not ask whether it is your turn, and so cannot refuse a caller who asks off-turn. Recorded as
a question for the engine's owner rather than a fault: the alternative is a caller-supplied "it is
my turn" statement on an entry whose evidence makes it a condition.
