# 0007 — Brandon's rulings on six open questions, five in the overlay and one the engine's own, are ruleset version 7

**Status:** accepted, 2026-09-16. Decided by Brandon, the engine's owner, on 2026-09-15 and
2026-09-16. Follows [0006](0006-a-mount-is-a-fact-the-caller-states-and-the-gaps-this-batch-closes.md),
which took the ruleset to version 6. Supersedes 0005 and 0006 where they record that
`group-initiative`, `next-round`, `appropriate-anatomy` and `mount-control-requires-training`
decline; the rest of both stands.

## Context

This engine has had no rulings. Every question `RulesFactory.Maps.Srd52Combat` 2.0.0 records as
`ambiguity.fate: unresolved` has been declined, and `MAP-FINDINGS.md` has listed four of them
(findings 6, 11, 17 and 21) as questions worth an owner's ruling. Since rules-factory 0.6.0 and
[its decision 0027](https://github.com/brandonifco/rules-factory/blob/main/docs/decisions/0027-an-owners-ruling-is-held-by-the-engine-and-checked-by-the-factory.md),
with its amendment of 2026-09-15, an owner's ruling on such a question is held in the engine's
overlay, quoting the span of the question it answers; the factory checks it against the map,
generates `Generated/Rulings.g.cs` from it, and records it in `provenance.json`. This record is the
first use of that mechanism here.

**On 2026-09-15 and 2026-09-16 Brandon ruled on six questions.** Each is his answer where the SRD
says nothing. **None of them is in the corpus, and none is presented as the corpus's.**

1. **Group initiative.** Every creature rolls its own Initiative. The engine does not group, whatever
   the caller states about identical creatures.
2. **The next round against combat ending.** If both sides agree to stop, combat ends, even though
   neither side is defeated.
3. **Disengage.** Its protection covers your own movement for the rest of your turn, following the
   Rules Glossary, not indefinitely.
4. **Moving through a creature.** "Two sizes larger or smaller" means two or more: a Medium creature
   may pass through a Huge or a Gargantuan one.
5. **An appropriate anatomy.** It is the GM's call. The caller supplies it explicitly, in the shape
   `gm-requires-action`'s GM assertion has. The engine never assumes it, and a mounting with no GM
   statement declines.
6. **Trained to accept a rider.** It is a caller-supplied fact, as a Swim Speed is. The creatures the
   corpus names — a domesticated horse, a mule — still work without being told.

### Which of them 0027 can carry, and which it cannot

0027 § 5.1 allows a ruling only on an entry the **map** records as `fate: unresolved` and the
**overlay** records as `implemented`. Each of the six was checked against map 2.0.0 before anything
was written:

| Ruling | Entry | `ambiguity.fate` | Carried as |
|---|---|---|---|
| 1 | `group-initiative` | `unresolved` | overlay ruling `group-initiative/no-grouping` |
| 2 | `next-round` | `unresolved` (a `conflict`, recorded as an unresolved question) | overlay ruling `next-round/agreement-ends-it` |
| 3 | `opportunity-attack-avoidance` | **none — `clarity: clear`** | this engine's own decision (below) |
| 4 | `moving-through-creatures` | `unresolved` | overlay ruling `moving-through-creatures/two-or-more` |
| 5 | `appropriate-anatomy` | `unresolved` | overlay ruling `appropriate-anatomy/gm-decides` |
| 6 | `mount-control-requires-training` | `unresolved` | overlay ruling `mount-control-requires-training/training-is-stated` |

Two of these needed care.

- **Ruling 2 is one question the map records twice.** `next-round` and `combat-end` carry the same
  question, in opposite words, joined by `ambiguity.conflict: does-combat-end-without-a-defeat`
  (rules-factory 0007: a conflict is a question, and each side of it carries it). A ruling is held per
  entry, and `combat-end` is **not implemented** in this engine's overlay, so 0027 cannot carry a
  ruling there: it declines `UnsupportedRule` as an unbuilt in-scope entry, as before. The ruling
  therefore sits on `next-round` alone, which is the entry that answers. When `combat-end` is built,
  it takes a second ruling, with the same answer, its own id and its own span, and this record is
  the one both name. A conflict being `fate: unresolved` is what makes a ruling possible at all: the
  question is the map's, not a note.
- **Ruling 3 has no unresolved question to sit on.** `opportunity-attack-avoidance` is
  `clarity: clear`. The slice's sentence ("You can avoid provoking an Opportunity Attack by taking
  the Disengage action") states no limit; the Rules Glossary's `disengage-action` (p. 181,
  `scope: out`) limits the protection to "your movement" and to "the rest of the current turn"; and
  the map records the difference in the entry's `note` and names the glossary entry in `dependsOn`
  and `crossReferences` (rules-factory 0026), without asking a question. `MAP-FINDINGS` finding 11
  is that record. So there is no `ambiguity.question` to quote a span of, `rulings` on that item
  would be refused by `rulings.py`, and **this is not a 0027 ruling.** It is the engine's own
  recorded decision, carried by `OwnerDecision`, a type of this engine's that is deliberately not
  `OwnerRuling`: a different claim with a different carrier, so nothing can mistake one for the
  other or for the corpus. **No finding is filed against the map either**: the map is right that the
  slice's sentence is clear, and it is the narrower glossary rule, which the map already points at,
  that the owner chose to follow.

## Decision

**The engine applies all six, names them on every result that relies on one, carries them through
every result derived from such a result, and the ruleset is `srd-5.2.1-combat` version 7.**

### The five overlay rulings (0027)

Each quotes its span of the entry's `ambiguity.question` verbatim. Four items carry
**`declines: []`**, declaring the question fully ruled. One, `moving-through-creatures`, keeps a
decline beside its ruling.

- **`group-initiative/no-grouping`** — "Every creature rolls its own Initiative: the engine never
  groups identical creatures, whatever the caller states." The whole question is ruled, because what
  makes creatures "a group of identical creatures" no longer decides anything the engine does: there
  is one d20 per participant and no group roll. The caller's `IdenticalCreaturesStatement` is still
  demanded and recorded — it is a fact of the table, and dropping it would hide what the engine was
  told — and it changes no draw. `GroupInitiativeRolls.Draws` stays 0: the roll is
  `initiative-roll`'s.
- **`next-round/agreement-ends-it`** — "Combat ends: where both sides agree to end it and neither
  side is defeated, no further round begins." The fourth case of `next-round` answers for the first
  time. The other three did not turn on the question and are unchanged (MAP-FINDINGS finding 14).
- **`moving-through-creatures/two-or-more`** — "Two or more: a creature two or more sizes larger or
  smaller than you may be passed through, so a Medium creature passes through a Huge or a Gargantuan
  one." That is the first half of the question. **The second half still declines**: the sentence
  permits passage through four kinds of space without saying that every other creature's space is
  impassable, and the owner did not rule on it, so a creature fewer than two sizes away that is
  neither Tiny nor an ally nor Incapacitated still declines `RequiresInterpretation`. This is the one
  item with a non-empty `declines`.
- **`appropriate-anatomy/gm-decides`** — "The GM decides: an appropriate anatomy is the GM's call,
  stated to the engine, and the engine never assumes one." The question names nobody who decides;
  the ruling names the GM, and that answers all of it. What remains is not a part of the question
  but a **missing fact**: where the GM has stated nothing, the engine declines
  `RequiresInterpretation` citing the entry, exactly as it declines any rule it has not been told
  what it needs. That decline names no ruling (0027 § 4: a decline relies on none), and the
  `UnresolvedResult` it gives carries no rulings field to name one in.
- **`mount-control-requires-training/training-is-stated`** — "The caller states it: training to
  accept a rider is a fact the caller supplies, and the horse and the mule the corpus names are
  trained without being told." The corpus's own instances answer as they did, **naming no ruling**,
  because the corpus settles them. Any other creature answers from the caller's statement, naming the
  ruling; with no statement it declines, as above.

### The engine's own decision

- **`OwnerDecisions.DisengageCoversYourOwnMovementThisTurn`**, on `opportunity-attack-avoidance`:
  "The Disengage action's protection covers your own movement for the rest of your turn, following
  `disengage-action` (p. 181), and no longer." `LeavingReach` gains what the ruling needs: whether
  the departure is the creature's own movement on the turn it Disengaged. A creature that Disengaged
  and then leaves reach by its own movement on a later turn **provokes**, where before it did not.
  The other two limbs of the sentence — Teleporting, and being moved without using your own
  movement, action, Bonus Action or Reaction — are the slice's own and are unchanged; they name no
  decision, and neither does a plain walk out of reach.

### How a ruling reaches the caller

0027 § 4 and its amendment of 2026-09-15 govern this, and the amendment is the case here as often
as § 4 itself:

- **`OwnerRulings`** is the generated registry (`Generated/Rulings.g.cs`, five statics in overlay
  order). `src/Srd52Combat/OwnerRulings.cs` adds names that say what each rules, and `InOrder`, which
  puts any set of rulings in overlay order without repeats, so every result lists them the same way.
  Nothing restates the metadata: the overlay is its one home.
- **Every answer that relies on a ruling carries it in a `Rulings` property**, and an answer that
  relies on none carries an empty one. `GroupInitiativeRolls`, `InitiativeRolls`, `NextRoundOutcome`,
  `PassageRuling`, `AnatomyRuling` (new), `MountEligibility`, `MountControl` and `ControlledMountTurn`
  each gained one. `ProvocationVerdict` and `OpportunityAttackOffer` carry `Decisions` instead,
  because what they rely on is the engine's decision and not a 0027 ruling.
- **Derived results carry their inputs' rulings** (the amendment). `initiative-roll` rolls a stated
  group only because `group-initiative` is ruled, and its `InitiativeRolls` names that ruling.
  `mount-eligibility` names `appropriate-anatomy/gm-decides` when the anatomy limb is what decided
  it. `controlled-mount-turn` names whatever `mount-control-requires-training` relied on.
  `opportunity-attack` names whatever `opportunity-attack-avoidance` relied on.
- **A decline names nothing.** `UnresolvedResult` has no rulings, and no decline's `Attempted` text
  claims a ruling as the corpus's.
- **Where a ruling is not carried.** `initiative-order`, `initiative-ties` and
  `initiative-ties-uncovered` take bare `InitiativeCount`s, as their requests do; a caller who orders
  counts that came from a ruled roll holds that ruling in the `InitiativeRolls` they came from. The
  same holds for `MovementBudgetRules`, which takes bare distances. This is the case 0027's amendment
  warns about, and it is named here rather than left implicit.
- **The seeded replay record carries them.** `SeededInitiativeReplayTests` renders a `rulings` line
  in the recorded bytes — the rulings the rolls relied on, in overlay order, or `none`. That is a new
  line in the record, so the record's shape changed and **the replay schema is 2**
  (`hoyle-backgammon`'s decision 0010 is the precedent: a new member of the record is a schema bump).

### What the ruleset version means

**`implementedIn` moves to 7 on every implemented entry**, for the reason 0006 gives: the version
names the engine's behaviour as a whole, and six of its answers changed. The seeded Initiative
replay's pinned SHA-256 moves with the identity line and the new `rulings` line; it is re-pinned
here, and a tool comparing records must refuse to compare a version 7 record with a version 6 one,
or a schema 2 record with a schema 1 one.

## Consequences

- **What no longer declines**: a stated group of identical creatures (`group-initiative`, and so
  `initiative-roll`, which now rolls it); a round after which both sides agreed and neither is
  defeated (`next-round`); passing through a creature more than two sizes larger or smaller
  (`moving-through-creatures`); a mount the GM has called appropriately shaped (`appropriate-anatomy`,
  and so `mount-eligibility`); and a creature the caller states is trained
  (`mount-control-requires-training`, and so `controlled-mount-turn`).
- **What still declines, and why**:
  - `moving-through-creatures`, for a creature fewer than two sizes away that is neither Tiny nor an
    ally nor Incapacitated: the second half of its question, not ruled on.
  - `appropriate-anatomy` and `mount-control-requires-training`, where the GM or the caller has
    stated nothing: the ruled behaviour, not a decline of the question.
  - `independent-mount` and `falling-off`, whose questions nobody has ruled on; `combat-end`,
    `side-defeated`, `sides-agree-to-end`, `surprised`, `round-duration`,
    `brief-or-extended-communication`, `difficult-terrain` and the rest of the unbuilt in-scope
    entries; `grid-entering-square`'s occupied square, `grid-range`, `cover-degree` below half
    cover, `total-cover`, `initiative-ties-uncovered` and `communication-cost`'s reach question, all
    as before.
  - Every `scope: out` entry, unchanged.
- **A creature that Disengaged provokes on a later turn.** That is a behaviour change on an entry
  the map calls clear, made by the engine's owner and recorded here, not a reading of the slice.
- **Source-breaking changes**: `GroupInitiativeRolls`, `InitiativeRolls`, `NextRoundOutcome`,
  `PassageRuling`, `MountEligibility`, `MountControl`, `ControlledMountTurn`, `ProvocationVerdict`
  and `OpportunityAttackOffer` each gained a positional member; `LeavingReach` gained one;
  `appropriate-anatomy`'s entry point answers an `AnatomyRuling` where it used to decline always;
  `MountCreatureStatement.AnotherCreature` takes the training the caller states.
  `MountRules.AppropriateAnatomy` and `MountRules.Control` take the new statements.
- **If a later map version settles any of these questions**, the engine follows the map and withdraws
  that ruling in a new record (0027 § 6). If it rewords one, the span stops matching and the merge is
  refused until Brandon confirms the ruling answers the question as it now reads.
