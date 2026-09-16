# 0005 — The turn and the round are the caller's statement, the gaps are its facts, and a group of identical creatures declines

**Status:** accepted. Ruleset `srd-5.2.1-combat` version 4 → 5. Amends [0001](0001-initiative-is-rolled-only-where-the-corpus-fixes-the-draws.md) for `initiative-roll`'s answer to a stated group.

## Context

Ten entries of `RulesFactory.Maps.Srd52Combat` 2.0.0 are built here: the turn ("Combat / Your Turn",
pp. 13–14) — `turn-move-and-action`, `break-up-move` (p. 14), `doing-nothing`,
`free-object-interaction`, `communication-cost` and the assertion `gm-requires-action` — and the
round — `combat-steps` ("Combat / Combat Step by Step / p. 13"), `next-round` ("Combat / The Order
of Combat / p. 13"), and, on p. 13's Initiative, `surprise-disadvantage` and `group-initiative`.

Four of them sit beside an entry the map split out to hold a gap, and three carry an unresolved
question of their own:

| Entry | The gap or question, and where the map puts it |
|---|---|
| `surprise-disadvantage` (clear) | `surprised`: when a combatant is surprised. A separate entry, `fate: unresolved` |
| `communication-cost` (ambiguous) | `brief-or-extended-communication`: where brief ends. A separate entry, unresolved — **and** its own question, whether `gm-requires-action` reaches communication |
| `next-round` (ambiguous) | `side-defeated`: what defeat is. A separate entry, unresolved — **and** its own question, the conflict with `combat-end` |
| `group-initiative` (ambiguous) | what makes creatures "a group of identical creatures". **Not** split out: the question is on this entry, and `affectsDraws: true` |

Four dependencies are `scope: out` (`actions-table`, `bonus-actions`, `reactions`,
`initiative-score-option`), and four are in scope and not built (`combat-rounds`, `move-up-to-speed`,
`movement-deduction`, `sides-agree-to-end`).

## Decision

**A turn is the steps the caller states, and the engine counts the budget.** `TurnStep` is a move of
so many feet, or an action, Bonus Action or Reaction named in the caller's words.
`turn-move-and-action` answers a `TakenTurn`: the distance moved, what is left of the Speed, how many
actions were taken, which step came first, and `Exceeded` — what the turn allows and these steps went
past (a move beyond the Speed, a second action). It judges the budget and nothing else: what an
action *is* is `actions-table` (p. 9), outside the slice, so an action is counted and never
interpreted, and a Bonus Action or a Reaction costs neither the move nor the action, because whether
a creature has one is `bonus-actions` and `reactions` (p. 10), also outside it. Moving first and
acting first give the same budget, which is the second sentence of the evidence.

**A broken-up move is deducted from one Speed.** `break-up-move` answers each part of the move, what
it follows, and the movement left after it — the remainder, never a fresh Speed. The corpus's own
example (Speed 30: 10 feet, an action, 20 feet) is a test, and so is the same turn with 30 feet after
the action, which goes past the Speed.

**Forgoing is permitted and creates no delay.** `doing-nothing` answers what the turn forgoes
(the move, the action, or everything) and that it is permitted. The second sentence of the span
("consider taking the defensive Dodge action or the Ready action to delay acting") is advice: the
engine has no delay, and neither Dodge nor Ready is a rule here.

**`gm-requires-action` is the GM's own determination, checked against the map.** Its value is a
`GmActionRequirement`: the party, the activities an action is required for (each an object
interaction or a communication, in the words the caller uses for them elsewhere), and who stated it.
The party must be one the map's `assertedBy` names, through `ActionRequirers.Named`, exactly as a tie
break's decider is (0002). The measure the corpus states — "when it needs special care or when it
presents an unusual obstacle" — is the GM's to apply, and the engine never applies it. The
requirement is demanded of any caller resolving the two entries it gates, and never defaulted in
either direction.

**The free object interaction is the first that is not paid for another way.** An interaction the
GM's requirement names costs an action and cites `gm-requires-action` (p. 14): the gate holds, so
this rule does not make it free. So does an object whose own description always requires an action, a
fact the caller states from the item's description (magic items, p. 204 onward, are outside the
extent). Neither spends the turn's one free interaction; the first interaction after them is free,
during the move or during the action, and the next needs the Utilize action.

**Communication is classified by the caller and costed by the rule.** Brief is free, extended
requires an action. Which one a given utterance is, is `brief-or-extended-communication`, a gap, so
the caller states it. Where the GM's requirement names that very communication, the answer turns on
the entry's own question — whether "any of these activities" reaches communication at all — and the
engine declines `RequiresInterpretation` citing `communication-cost` (p. 13). A requirement naming
only object interactions, or some other communication, leaves the rule untouched.

**No turn is taken before Initiative is rolled.** `combat-steps` answers the three steps in the
corpus's order. Step 1 is the caller's `PositionsStatement`, recorded and attributed: positions are
facts the later rules test, and the GM supplying them decides nothing. Step 2 is the `TurnOrder`
`initiative-order` resolved, and it is demanded: step 3's turns are that order's, so with no order
there is no sequence and the engine infers none (`ArgumentException`). Whether the fighting stops is
not answered here — "Repeat this step until the fighting stops" names no rule of its own.

**`next-round` answers three of its four cases and declines the fourth.** Before everyone has taken a
turn the round is not over and no round follows yet. Once everyone has: with neither side defeated
the fight continues to the round after this one; with a side defeated it does not. Where neither side
is defeated *and* both sides have agreed to end combat, p. 13 continues the fight and p. 14 ends the
combat, and the engine declines `RequiresInterpretation` citing `next-round`, naming `combat-end` in
the attempt. Whether a side is defeated (`side-defeated`, a gap) and whether both sides have agreed
(`sides-agree-to-end`, not built) are facts the caller states, as `surprised` is; a combatant taking
two turns in a round, or one outside the order, is refused.

**Surprise gives Disadvantage and nothing else.** Given the caller's statement that a combatant is
surprised, `surprise-disadvantage` answers a `SurpriseEffect` whose `InitiativeRoll` is
`D20Mode.Disadvantage` — the mode the caller then states to `initiative-roll`, which declines it
citing p. 7, as it declines every modified roll (0002). For a combatant that is not surprised the
effect is **null, not `D20Mode.Straight`**: a roll's mode comes from every source, and this rule
speaks only of surprise. Where the GM uses Initiative scores there is no roll to have Disadvantage
on, and what Disadvantage does to a score is in the glossary, so the rule declines
`OutsideCurrentScope` citing `initiative-score-option` (p. 184), the gate `initiative-roll` answers
to as well.

**`group-initiative` answers who rolls and declines how many rolls, and `initiative-roll` follows
it.** "The GM rolls for monsters" is answered: the GM rolls for every participant the caller states
is a monster. The second sentence cannot be. What makes creatures a group of identical creatures is
nowhere in the corpus — the same stat block or not, one group or several, a lone monster a group of
one — and the map records it as *a gap, not a delegated choice*, with `affectsDraws: true`. So the
engine does not let the caller settle it either: a stated group declines `RequiresInterpretation`
citing `group-initiative`, and nothing is drawn. Map 2.0.0's `draws` for `initiative-roll` is one d20
per participant "not in a group of identical creatures, whose roll is group-initiative's", so
`initiative-roll` declines a stated group the same way. That changes its answer from
`UnsupportedRule` ("group-initiative … is not built") to `RequiresInterpretation`: the rule is built
now, and what it says is that the count is not fixed. Nothing that resolved before declines now, and
nothing that declined before resolves.

**The ruleset is `srd-5.2.1-combat` version 5**, and the eleven entries whose code this batch wrote
or changed say so in the overlay (`initiative-roll` among them). The replay's pinned SHA-256 moves
from `20fb2e9b…88f9` to `c92429e3…dfb2`: only the rendered identity line changes, as in 0002.

## Consequences

- A combat containing any group of identical creatures cannot be rolled by this engine at all, by
  either entry, until the question is settled. That is the price of `affectsDraws: true` on an
  unresolved question, and it is the honest one for a seeded engine.
- The movement half of a turn is implemented here, in `turn-move-and-action`. `move-up-to-speed` and
  `movement-deduction` (pp. 14) restate it and are not built; when they are, they must agree with
  `TurnRules`, not repeat it.
- `TurnRules` counts feet the caller gives it. What a foot of movement costs — Difficult Terrain,
  a mode of movement, a grid square — is elsewhere (`difficult-terrain-cost`, `movement-modes`,
  `grid-entering-square`), and the caller brings the totals here.
- An interaction the GM requires an action for does not spend the turn's free interaction. The corpus
  does not say so in as many words; it follows from the gate suspending this rule for that activity,
  and it is the reading recorded here.
