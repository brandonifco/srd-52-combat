# Where the map turned out to be wrong

A running log, kept while building this engine from `RulesFactory.Maps.Srd52Combat`. One section
per finding: what the map says, what implementing it revealed, and whether the map or the code is
at fault. Quotations of SRD 5.2.1 here are under the corpus's licence, CC-BY-4.0; see `NOTICE`.

Counts so far: **11 findings, from two batches against map 2.0.0. Two are a fault in the map**,
both in the attacks batch (7, 8). Findings 1-6 are the movement and space batch's (ruleset version
3): four are records of something the map is right about and that a reader of the engine would
otherwise have to rediscover (1, 3, 4, 5); one is a gap in the method rather than in the map (2);
one is a question for the engine's owner (6). Findings 7-11 are the attacks batch's (ruleset
version 4): two faults (7, 8), two records (9, 10) and one more question for the owner (11).
Neither batch found an entry that was the wrong *unit* of work.

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

---

The attacks batch, [decision 0004](docs/decisions/0004-an-attack-names-what-lies-outside-the-slice-and-performs-what-does-not.md).

## 7. `attack-resolution` declares draws that no engine of this slice can make

*The map is at fault.* The entry carries

```json
"draws": [
  { "dice": "d20", "count": "one attack roll; two d20s with Advantage or Disadvantage" },
  { "dice": "damage dice", "count": "the attack's own damage dice, on a hit only, unless the attack's rules say otherwise" }
]
```

and `dependsOn: ["attack-rolls", "damage-rolls"]`, both of which are `scope: out`. A d20 drawn
without `attack-rolls` cannot be read against anything, and the damage dice are the particular
attack's, outside the extent entirely. So an engine built to this map's extent draws nothing here,
and the entry's own `note` agrees: "In the slice: damage is rolled on a hit and only on a hit,
unless the attack's own rules say otherwise." `draws` is describing an engine larger than the slice
the map draws. Either the field should be conditioned on its drawing rules being in scope, or the
method needs a way to say "these are the draws the rule would make, made elsewhere".

The engine follows the `note`: `attack-resolution` takes the roll's outcome as a statement the
caller makes, says whether damage is rolled, and on a hit names `damage-rolls` (p. 16) as the
authority for the roll the caller must then make.

## 8. `wrong-location-misses`' note asserts a draw the entry does not declare

*The map is at fault, mildly.* The note says "The attack roll is made, and the attack misses
whatever it shows … A seeded engine draws for it." The entry carries no `draws`. Nothing is
inconsistent if the draw belongs to `attack-resolution`, which is where the roll is made and which
does declare it — but a note that says a seeded engine draws, on an entry with no `draws`, reads
as a contradiction of decision 0025 until you find the other entry. The engine draws nothing for
this entry; it answers that the attack misses.

## 9. `attack-modifiers` can only decline until `cover-degree` is built

*The map is right; the record is worth keeping.* Step 2 determines three things, and one of them —
whether the target has Cover — is `cover-degree`, in the map and not built. So the step cannot be
completed however good the rest of the engine is, and `attack-modifiers` declines
`UnsupportedRule` citing `cover-degree`, with the Advantage and Disadvantage it did determine named
in the decline. That is the map being honest about a dependency, not a defect. It is recorded here
because "implemented" and "no answering path" look like a contradiction in the overlay, and decision
0003 explains why the entry is `implemented` rather than `blocked`.

The same shape, less sharply: `attack-target` declines for a melee attack (`melee-within-reach`)
and for a ranged attack with one range (`single-range`), and answers only for a ranged attack with
two ranges (`normal-and-long-range`), which this batch built.

## 10. `dependsOn` cannot say "one of these, whichever the attack is"

*The map is right; the method has no field for what was found.* `attack-target`'s `dependsOn` is
`["single-range", "normal-and-long-range", "melee-within-reach"]`. Those three are alternatives, not
three conditions that hold together: exactly one applies to any given attack, decided by what the
attack is. `dependsOn` reads as a conjunction everywhere else in the map, and nothing distinguishes
this case. The backlog's build order is right either way, so nothing went wrong; but a reader
cannot tell from the field that `attack-target` answers as soon as *any one* of the three is built.

## 11. The slice's avoidance sentence is broader than the glossary's Disengage

*The map is right, and the question it leaves open is worth a ruling.* The slice says "You can
avoid provoking an Opportunity Attack by taking the Disengage action", full stop. The Rules
Glossary's Disengage (`disengage-action`, p. 181, `scope: out`) limits the protection to "your
movement" and to "the rest of the current turn". The map records the difference in
`opportunity-attack-avoidance`'s `note` and names `disengage-action` in `dependsOn` (0026), but
leaves `clarity: clear` and asks no question, so the engine implements the sentence the slice
states: a creature that took the Disengage action does not provoke.

Whether that is right — whether a creature that Disengages, and is then hurled back into reach and
out of it again on someone else's turn, is still protected — is not a case the slice decides, and
the engine's answer comes from the narrower text not being in it. It is listed here rather than
guessed at.
