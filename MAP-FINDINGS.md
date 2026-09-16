# Where the map turned out to be wrong

A running log, kept while building this engine from `RulesFactory.Maps.Srd52Combat`. One section
per finding: what the map says, what implementing it revealed, and whether the map or the code is
at fault. Quotations of SRD 5.2.1 here are under the corpus's licence, CC-BY-4.0; see `NOTICE`.

Counts so far: **22 findings, from four batches against map 2.0.0. Three are a fault in the map**,
two in the attacks batch (7, 8) and one in the mounted-combat batch (18). Findings 1-6 are the
movement and space batch's (ruleset version 3), 7-11 the attacks batch's (version 4), 12-16 the turn
and round batch's (version 5), and 17-22 the mounted-combat, underwater and gap-closing batch's
(version 6). Across them: three faults, eleven records of something the map is right about that a
reader would otherwise have to rediscover, four gaps in the method rather than in the map, and four
questions for the engine's owner (6, 11, 17, 21). No batch has found an entry that was the wrong
*unit* of work.

**Brandon ruled on six questions on 2026-09-15 and 2026-09-16** (ruleset version 7,
[decision 0007](docs/decisions/0007-brandons-rulings-on-six-open-questions-are-ruleset-version-seven.md)).
Five are owner's rulings held in this engine's overlay under rules-factory 0027; the sixth,
Disengage, is the engine's own recorded decision, because the entry it concerns records no open
question for a ruling to sit on. **None of them is a finding against the map**: each answers where
the corpus says nothing, and every map entry stays as it was. They close findings 11, 12, 14, 17 and
21, and the ruled half of 3; each of those sections says below what it now records. **Finding 6 is
still open**: nobody has ruled on whether `dropping-prone` should ask whose turn it is.

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

*Ruled by the owner (version 7, [decision 0007](docs/decisions/0007-brandons-rulings-on-six-open-questions-are-ruleset-version-seven.md)):* "two sizes larger or smaller" means two or more
(`moving-through-creatures/two-or-more`), so the limb now reaches a Gargantuan creature for a Medium
one, and the smaller half reaches Tiny for a Huge one. The observation stands for the exactly-two
cases the tests still demonstrate.

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

*Still open at version 7.* The rulings of 2026-09-15 and 2026-09-16 did not reach this one, and
`dropping-prone` still answers the permission without asking whose turn it is.

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

*Closed by the mounted-combat, underwater and gap-closing batch (version 6):* `cover-degree`,
`melee-within-reach` and `single-range` are built, and both entries answer. `attack-modifiers` still
declines where `cover-degree` does, which is a creature covering less than half of the target, or a
target behind more than one source of cover (`cover-no-stacking`, not built).

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

*Answered, and not by a 0027 ruling (version 7, [decision 0007](docs/decisions/0007-brandons-rulings-on-six-open-questions-are-ruleset-version-seven.md)).* Brandon decided on 2026-09-16 that the engine
follows the glossary: the Disengage action's protection covers your own movement for the rest of your
turn, and no longer. Because this entry is `clarity: clear` and carries no `ambiguity.question`,
there is no span for an owner's ruling to quote, and `rulings` on its overlay item would be refused
by the factory. So it is held as this engine's own `OwnerDecision`, named on every verdict and offer
that rests on it, and the map is unchanged: the slice's sentence really is clear, and it is the
narrower glossary rule — which the map already names in `dependsOn` and `crossReferences` — that the
owner chose to follow.

---

## 12. An unresolved question with `affectsDraws: true` takes two entries out of service

*The map is right; the consequence is worth seeing.* `group-initiative`'s question is what makes
creatures "a group of identical creatures", marked `affectsDraws: true`: the answer fixes how many
d20s a combat throws. `initiative-roll`'s `draws` in map 2.0.0 says one d20 per participant "not in
a group of identical creatures, whose roll is group-initiative's". Put together, a combat in which
the caller states any group cannot be rolled by either entry: `group-initiative` declines the count,
and `initiative-roll` cannot roll the rest without it. The two `draws` fields are what make that
visible; nothing in a backlog item says that building one entry of such a pair changes the other's
answer. Building `group-initiative` moved `initiative-roll`'s stated-group decline from
`UnsupportedRule` to `RequiresInterpretation`
([decision 0005](docs/decisions/0005-the-turn-and-the-round-are-stated-by-the-caller-and-a-group-of-identical-creatures-declines.md)).

*Closed by the owner's ruling (version 7, [decision 0007](docs/decisions/0007-brandons-rulings-on-six-open-questions-are-ruleset-version-seven.md)).* Brandon ruled on 2026-09-15 that every creature rolls
its own Initiative and this engine never groups (`group-initiative/no-grouping`). Both entries answer
again: `group-initiative` records the statement and gives the GM no roll, `initiative-roll` throws one
d20 per participant, and each names the ruling. The shape the finding describes — an unresolved
question with `affectsDraws: true` taking two entries out of service — is real, and an owner's ruling
is what put them back.

## 13. Nothing in the method says what an entry answers when an *in-slice* gate holds

`free-object-interaction` and `communication-cost` name `gm-requires-action` in `suspendedBy`. The
method fixes the runtime answer for a gate outside the slice — `OutsideCurrentScope` citing the gate
(rules-factory 0021) — but this gate is `scope: in`, `kind: assertion`, and built. This engine
answers the gate's own rule: the activity requires an action, cited to `gm-requires-action` (p. 14).
That reads well because the gate's evidence states a consequence ("might require you to use an
action"); a gate whose evidence stated only a condition would leave the same hole the movement batch
found for `enabledBy` (finding 2). The map is not wrong; the method has no field for it.

## 14. Half of `next-round` is unaffected by the question recorded on it

`next-round` is `clarity: ambiguous`, `fate: unresolved`, on the conflict with `combat-end`: what
happens when both sides agree and neither is defeated. Three of its four cases — the round not yet
over, over with neither side defeated, over with a side defeated — do not turn on that question and
are answered. An entry marked unresolved is not an entry that can only decline, and the acceptance
criterion says as much ("where the answer turns on the question"). Worth recording because the shape
recurs: `communication-cost` and `group-initiative` in this batch each answer one half and decline
the other.

*Closed for `next-round` by the owner's ruling (version 7, [decision 0007](docs/decisions/0007-brandons-rulings-on-six-open-questions-are-ruleset-version-seven.md)).* Brandon ruled on 2026-09-15 that
combat ends where both sides agree and neither is defeated (`next-round/agreement-ends-it`), so the
fourth case answers too. The same question sits on `combat-end`, which is not built: a ruling is held
per entry, and only on an entry the overlay marks `implemented`, so that entry carries no ruling yet
and still declines as an unbuilt in-scope rule. When it is built it takes a second ruling, with the
same answer and its own span.

## 15. Part of `free-object-interaction`'s span is a rule the extent does not cover

The entry's evidence ends "Some magic items and other special objects always require an action to
use, as stated in their descriptions", and its one `crossReference` is `unmapped`: the item's own
description is the rule, and magic items (p. 204 onward) are outside the extent. The sentence is
still a rule about the turn's free interaction, and an engine that dropped it would call a wand free.
This engine takes "this object's description always requires an action" as a fact the caller states,
answers `RequiresAction` and cites p. 13. The `unmapped` note is what makes that reading available;
a reader of the entry alone might have dropped the sentence with the cross-reference.

## 16. `turn-move-and-action` and `move-up-to-speed` state the same rule on two pages

The map says so in `move-up-to-speed`'s note ("Restates turn-move-and-action's movement half, in
agreement"), and both are `scope: in`. This batch built the turn entry, so the movement budget — a
distance up to the Speed, or none — now lives in `TurnRules`. When `move-up-to-speed` and
`movement-deduction` are built they must answer through that rule rather than restate it, or the
engine will hold two implementations of one sentence. The map is right; the risk is in the build
order, which no field records.

---

The mounted-combat, underwater and gap-closing batch,
[decision 0006](docs/decisions/0006-a-mount-is-a-fact-the-caller-states-and-the-gaps-this-batch-closes.md).

## 17. Nothing in the map can ever reach the mounted-combat rules

*The map is right, and the question it leaves open is worth a ruling.* `appropriate-anatomy` is
`clarity: ambiguous`, `fate: unresolved`, and its question ("an appropriate anatomy" states no
measure, no set of values, and nobody who decides) is the whole of the entry: every asking of it
declines. `mount-eligibility` `dependsOn` it, so a willing creature of a large enough size gets a
decline and never a "yes". And every entry after it — `mounting-cost`,
`mount-control-requires-training`, `controlled-mount-turn`, `independent-mount`, `falling-off` —
names `mount-eligibility` or `mounting-cost` in `enabledBy`, which the backlog turns into "every
test of the rule sets up a state in which it holds".

So the five rules of mounted combat are unreachable from inside the engine, and the acceptance
criterion asks for a state the engine cannot produce. This engine takes the gate as a caller's
statement (`MountStatement`: not a mount, serves as a mount, ridden), by the same reading
`grid-play` got in finding 2, and declines `OutsideCurrentScope` where it is shut. That keeps the
engine from deciding the anatomy question, but it does mean a caller can put a rider on a giant
spider and the engine will price the mounting.

The ruling worth having: is "an appropriate anatomy" the GM's call, in which case the entry should
carry an `assertedBy` and the statement is an assertion rather than a parameter? Or is the gate
what this engine made it, a fact of the table's fiction that the engine records and never checks?

*Ruled by the owner (version 7, [decision 0007](docs/decisions/0007-brandons-rulings-on-six-open-questions-are-ruleset-version-seven.md)).* Brandon ruled on 2026-09-16 that it is the GM's call
(`appropriate-anatomy/gm-decides`): the caller supplies the determination explicitly, in the shape
`gm-requires-action`'s GM assertion has, and the engine never assumes one. `appropriate-anatomy` now
answers where the GM has determined something, naming the ruling, and declines where the GM has not;
`mount-eligibility` answers with it, so a chain of rules inside the engine does reach a creature that
serves as a mount. **The map is unchanged, and no `assertedBy` is filed as a finding**: that the GM
decides is Brandon's answer to a question the corpus leaves open, not something the corpus says, so
it belongs in this engine's overlay and binds no other engine built from the map. `MountStatement`
stays as decision 0006 made it.

## 18. `falling-off` declares a draw its own dependency puts outside the engine

*The map is at fault, in the same way as finding 7.* The entry carries

```json
"draws": { "dice": "d20", "count": "one Dexterity saving throw each time the rule applies" }
```

and `dependsOn: ["saving-throws", "prone-condition"]`, both `scope: out`. The DC is in the slice
(10) and the ability is named (Dexterity), but what is rolled against that DC — the d20 plus a
Dexterity saving throw modifier and whatever proficiency applies — is `saving-throws`', outside the
extent. A seeded engine of this slice that drew a bare d20 here would be comparing the wrong number
to the DC. So the engine draws nothing, states the DC and the ability, and takes the outcome as a
statement, exactly as `attack-resolution` does with the attack roll. As finding 7 asked: either
`draws` should be conditioned on the drawing rule being in scope, or the method needs a way to say
"this is the draw the rule would make, made elsewhere".

## 19. The three mounted-combat entries that share a page share a citation, and the id carries the work

*The map is right; the record is worth keeping.* `mount-control-requires-training`,
`controlled-mount-turn` and `independent-mount` all cite "Combat / Controlling a Mount / p. 16",
and `mount-eligibility` and `appropriate-anatomy` both cite "Combat / Mounted Combat / p. 15". A
decline's `Locator` therefore cannot say which of them declined, and a test that asserts only the
locator cannot tell `mount-eligibility` citing itself from `mount-eligibility` citing
`appropriate-anatomy` — the mutation that swapped them turned nothing red until the entry id was
asserted in `Attempted` as well. The engine follows `GridRules`' precedent and names the entry in
every decline; the finding is that the map's shape makes that mandatory here, not optional.

## 20. `underwater-ranged` and `normal-and-long-range` overlap beyond long range

*The map is right; the record is worth keeping.* `normal-and-long-range` says "you can't attack a
target beyond long range". `underwater-ranged` says a ranged weapon attack underwater "automatically
misses a target beyond the weapon's normal range", which includes everything beyond long range. The
two do not contradict each other — no hit either way — but they are different statements: one says
no attack is made, the other that an attack is made and misses. Nothing in the map marks the
overlap, and `dependsOn: ["normal-and-long-range"]` does not say which governs. The engine reports
both: the range verdict (`CanAttack: false`) and the automatic miss, and chooses between them
nowhere.

## 21. `mount-control-requires-training` names instances and no decider

*The map is right, and the question it leaves open is worth a ruling.* "Domesticated horses, mules,
and similar creatures have such training." The engine answers for the two the corpus names and
declines `RequiresInterpretation` for everything else, because "similar" states no measure and the
slice names nobody who may say a creature is trained.

The alternative would be to let the caller state training as a fact, the way this engine lets the
caller state a Swim Speed or the Incapacitated condition. The difference is that the corpus *does*
speak to training — it gives instances — and the question the map asks is precisely who extends the
list. A ruling from the engine's owner would settle whether "trained to accept a rider" is a
caller-supplied fact with an `assertedBy`, or a gap that stays declined until a corpus outside this
extent defines it.

*Ruled by the owner (version 7, [decision 0007](docs/decisions/0007-brandons-rulings-on-six-open-questions-are-ruleset-version-seven.md)).* Brandon ruled on 2026-09-16 that it is a caller-supplied fact,
as a Swim Speed is (`mount-control-requires-training/training-is-stated`). The creatures the corpus
names — a domesticated horse, a mule — still answer on the corpus's own words and name no ruling; any
other creature answers from the caller's statement and names the ruling, and declines where nothing is
stated. As with finding 17 the map does not move, and the engine files no `assertedBy` finding against
it.

## 22. `movement-deduction` prices a part of a move, not a fraction of one

*The map is right; the record is worth keeping.* "you deduct the distance of each part of your move
from it until it is used up or until you are done moving, whichever comes first." The engine deducts
each stated part whole, and a part the movement left does not cover is not taken, nor is anything
after it. The corpus does not describe taking half a part, and `move-up-to-speed` allows "a distance
equal to your Speed or less", so nothing licenses a total past the Speed. `mounting-cost`'s own note
reaches the same conclusion for the mounting cost ("a creature with less movement left than the cost
cannot mount or dismount"), which is what makes the reading safe; the general rule is what the
engine implements, and the note is the corroboration. Following finding 16, both entries answer
through `TurnRules`: `move-up-to-speed` asks `turn-move-and-action`'s rule whether the distance is
within the turn, and `movement-deduction` asks it of each part in turn, so the sentence the two
pages share has one implementation.
