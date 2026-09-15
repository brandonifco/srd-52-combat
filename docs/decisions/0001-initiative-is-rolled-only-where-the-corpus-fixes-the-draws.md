# 0001 — Initiative is rolled only where the corpus fixes the draws, and ties are the caller's statement

**Status:** accepted.

## Context

"Combat / Initiative / p. 13" is four entries of `RulesFactory.Maps.Srd52Combat` 1.0.0 that this
engine implements, and several it does not:

- `initiative-roll`, operation, clear. Every participant makes a Dexterity check. `dependsOn`
  `ability-checks` (p. 6, `scope: out`) and `suspendedBy` `initiative-score-option` (p. 184,
  `scope: out`), under which the GM uses Initiative scores and nothing is rolled.
- `initiative-order`, operation, clear. Highest to lowest, the same every round.
- `initiative-ties`, assertion. The GM orders tied monsters, the players tied characters, the GM a
  tie between a monster and a player character.
- `initiative-ties-uncovered`, operation, ambiguous, `fate: unresolved`. The tie rule does not say
  who orders a monster and a character that is not a player character, nor what happens when the
  players do not agree.
- Not built, and so still declining `UnsupportedRule`: `group-initiative` (one roll for a group of
  identical creatures; what a group is, is its unresolved question) and `surprise-disadvantage`.
  `advantage-disadvantage` is `scope: out` (p. 7).

The corpus declares `randomness: seeded`. A seeded engine that draws a different number of d20s
than the table would have changes every later draw of a replay (the map's README, finding 6).

## Decision

**The engine draws only where every fact that decides the number of draws has been stated.**
`InitiativeRollRequest` demands three statements and defaults none:

| Statement | When it says | Answer | Cites |
|---|---|---|---|
| `InitiativeScoreOptionStatement` | the GM uses Initiative scores | `OutsideCurrentScope`, nothing drawn | `initiative-score-option`, p. 184 (the gate, decision 0021 of rules-factory) |
| `IdenticalCreaturesStatement` | some participants form a group of identical creatures | `UnsupportedRule`, nothing drawn | `group-initiative`, not built |
| `Combatant.Roll` | a roll has Advantage or Disadvantage | `OutsideCurrentScope`, nothing drawn | `advantage-disadvantage`, p. 7 |

Otherwise one d20 is drawn per participant, in the order given, through
`UniformInt.InRange(source, 1, 20)` over the caller's `IRandomSource` (PCG32 in every test).

**The Dexterity check modifier is a parameter.** How it is composed (ability modifier, Proficiency
Bonus, circumstantial bonuses) is "D20 Tests", p. 6, outside the slice. The caller supplies the sum.

**Whether a creature is a monster, a player character or another character is a parameter** the
caller states on each `Combatant` and `InitiativeCount`.

**A tie break is an attributed statement.** The value of `initiative-ties` is a `TieBreaks`: one
`TieBreak` per tie, naming the decider (`Gm` or `Players`), the order, and who stated it. The engine
reads it only when there is a tie, and refuses it (`ArgumentException`) when it names the wrong
decider, other combatants, or a tie that does not exist. A missing one is
`AssertionRequiredException`. The order and the tie breaks are both recorded on `TurnOrder`.

**The uncovered cases decline `RequiresInterpretation`, citing `initiative-ties-uncovered`
("Combat / Initiative / p. 13").** These cases are a tie containing a monster and a non-player
character (three-way ties included), and a `TieBreak.PlayersDisagree` statement.
`initiative-ties-uncovered`'s own entry point resolves who decides each tie, and declines the first
case. `initiative-order` and `initiative-ties` decline both. Every Initiative entry cites the same
page, so `Attempted` names the entry.

**"Characters" includes non-player characters.** The tie rule gives ties "among tied characters" to
the players, and the map's question treats a non-player character as a character. A tie of player
and non-player characters, or of non-player characters alone, is therefore the players' to decide.
The engine does not ask whether the players control those characters.

## Consequences

- A surprised combatant's roll has Disadvantage. The engine does not decide who is surprised
  (`surprised` is a gap), so the caller states `D20Mode.Disadvantage`, and the roll declines.
- A combat with any identical monsters cannot be rolled here until `group-initiative`'s question is
  decided.
- A replay is comparable only under `Ruleset.Identity` (`srd-5.2.1-combat` v1, PCG32) and map
  1.0.0. `SeededInitiativeReplayTests` pins the SHA-256 of one recorded combat start.
