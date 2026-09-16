# SRD 5.2.1 Combat

A deterministic rules engine for the Combat chapter of the System Reference Document 5.2.1
(pp. 13–16), produced by [rules-factory](https://github.com/brandonifco/rules-factory) from a
corpus map, on [`RulesKernel`](https://www.nuget.org/packages/RulesKernel) 0.3.0.

The corpus is the SRD 5.2.1 as text extracted from the official PDF, pinned in
`corpus/srd-5.2.1.txt` and hashed on every validation run. The specification is the map in the
package [`RulesFactory.Maps.Srd52Combat`](https://www.nuget.org/packages/RulesFactory.Maps.Srd52Combat)
2.0.0: ninety-five entries, seventy in scope.

Citations are by heading path and printed page (`Combat / Initiative / p. 13`). The corpus declares
`randomness: seeded` (rules-factory decision 0019): the engine may draw random values, only through
`RulesKernel.Randomness`'s seeded PCG32 source, so that a combat replays from its seed.

## State

Fifty-four entries are implemented.

- **Initiative**, "Combat / Initiative / p. 13": `initiative-roll`, `initiative-order`,
  `initiative-ties` and `initiative-ties-uncovered` (ruleset version 2).
- **Movement and space** (ruleset version 3): `movement-modes`, `dropping-prone`,
  `size-categories`, and, for moving around other creatures (p. 14), `moving-through-creatures`,
  `creature-space-difficult-terrain`, `no-willing-end-in-occupied-space` and
  `ending-turn-in-occupied-space`.
- **The grid**, the optional variant of "Combat / Playing on a Grid / p. 13", which applies only
  when the caller states the table plays on a square grid: `grid-square-size`,
  `grid-speed-in-squares`, `grid-entering-square` and `grid-corners`.
- **Attacks** (ruleset version 4), "Combat / Making an Attack / pp. 14-15" with its "Unseen
  Attackers and Targets" sidebar, "Range", "Ranged Attacks in Close Combat" and "Opportunity
  Attacks": `attack-sources`, `attack-structure`, `attack-target`, `attack-modifiers`,
  `attack-resolution`, `unseen-attacker-advantage`, `unseen-target-disadvantage`,
  `wrong-location-misses`, `hidden-attacker-revealed`, `normal-and-long-range`,
  `ranged-in-close-combat`, `opportunity-attack` and `opportunity-attack-avoidance`.
- **The turn and the round** (ruleset version 5): "Combat / Your Turn", pp. 13-14 —
  `turn-move-and-action`, `break-up-move` (p. 14), `doing-nothing`, `free-object-interaction`,
  `communication-cost` and the GM's assertion `gm-requires-action`; and the shape of a combat —
  `combat-steps` (p. 13), `next-round` (p. 13), and, on Initiative, `surprise-disadvantage` and
  `group-initiative`.
- **Mounted combat** (ruleset version 6), "Combat / Mounted Combat / p. 15" through "Combat /
  Falling Off / p. 16": `mount-eligibility`, `appropriate-anatomy`, `mounting-cost`,
  `mount-control-requires-training`, `controlled-mount-turn`, `independent-mount` and
  `falling-off`.
- **Underwater** (ruleset version 6), "Combat / Impeded Weapons / p. 16" and "Combat / Fire
  Resistance / p. 16": `underwater-melee`, `underwater-ranged` and `underwater-fire-resistance`.
- **Cover, reach and the movement budget** (ruleset version 6), the entries the earlier batches
  declined citing: `cover-degree` ("Combat / Cover / p. 15"), `melee-within-reach`, `reach` and
  `single-range` ("Combat / Melee Attacks" and "/ Range", p. 15), and `move-up-to-speed` and
  `movement-deduction` ("Combat / Movement and Position / p. 14"), which answer through
  `TurnRules` and which `mounting-cost` spends. With them, `attack-modifiers` and `attack-target`
  answer for the first time (`implementedIn` version 6).

- **The owner's rulings** (ruleset version 7), which changed what six of those entries answer
  without changing the map: `group-initiative` and `initiative-roll` (every creature rolls its own
  Initiative), `next-round` (both sides agreeing ends the combat), `moving-through-creatures` ("two
  sizes larger or smaller" means two or more), `appropriate-anatomy` and `mount-eligibility` (the GM
  decides the anatomy), `mount-control-requires-training` and `controlled-mount-turn` (training is a
  fact the caller states), and `opportunity-attack-avoidance` and `opportunity-attack` (Disengage
  protects your own movement for the rest of your turn). See below and [decision 0007](docs/decisions/0007-brandons-rulings-on-six-open-questions-are-ruleset-version-seven.md).

Every other in-scope entry declines through its generated entry point with the reason the map's
correspondence table gives and its own citation. The rules still to build are listed in `backlog/`,
and what building these found about the map is in [MAP-FINDINGS.md](MAP-FINDINGS.md).

## The owner's rulings, and what is the corpus's

Five of this engine's answers rest on **an owner's ruling**: Brandon's answer to part of a question
the SRD leaves open, held in `corpus-map.overlay.json` under
[rules-factory decision 0027](https://github.com/brandonifco/rules-factory/blob/main/docs/decisions/0027-an-owners-ruling-is-held-by-the-engine-and-checked-by-the-factory.md),
generated into `OwnerRulings` and checked against the map on every `factory produce`. **A ruling is
never the corpus's**, so every result that relies on one carries it in a `Rulings` property — as does
every result derived from such a result — and a decline carries none.

| Ruling | Entry | What it settles |
|---|---|---|
| `group-initiative/no-grouping` | `group-initiative` | Every creature rolls its own Initiative; the engine never groups identical creatures |
| `next-round/agreement-ends-it` | `next-round` | Both sides agreeing ends the combat, though neither side is defeated |
| `moving-through-creatures/two-or-more` | `moving-through-creatures` | "Two sizes larger or smaller" means two or more |
| `appropriate-anatomy/gm-decides` | `appropriate-anatomy` | An appropriate anatomy is the GM's call, stated to the engine |
| `mount-control-requires-training/training-is-stated` | `mount-control-requires-training` | Training to accept a rider is a fact the caller supplies |

A sixth ruling has no such carrier. `opportunity-attack-avoidance` is `clarity: clear`, so the map
records no open question for a ruling to quote a span of, and the factory would refuse one there.
Brandon's decision that the Disengage action's protection follows the Rules Glossary — your own
movement, for the rest of your turn — is therefore **this engine's own recorded decision**, carried
by `OwnerDecisions`, a type deliberately distinct from `OwnerRuling`, and named the same way on every
answer that rests on it.

```csharp
var rolls = Value<InitiativeRolls>(EntryPoints.InitiativeRoll.Resolve(request));
foreach (var ruling in rolls.Rulings)
{
    // group-initiative/no-grouping, Brandon, 2026-09-15, docs/decisions/0007-....md
    Console.WriteLine($"{ruling.Id} (not the corpus): {ruling.Answer}");
}
```

## What is here

| | |
|---|---|
| `src/Srd52Combat/OwnerRulings.cs`, `OwnerDecisions.cs` | Names for the generated owner's rulings and the order results list them in; and this engine's own decision where 0027 has no carrier. Hand-written. |
| `src/Srd52Combat/Rules` | The rules: `InitiativeRules`, `MovementRules`, `MovementBudgetRules`, `GridRules`, `AttackRules`, `OpportunityAttackRules`, `TurnRules`, `RoundRules`, `SurpriseRules`, `GroupInitiativeRules`, `TargetingRules`, `CoverRules`, `MountRules`, `UnderwaterRules`. Hand-written. |
| `src/Srd52Combat/Handlers` | One file per implemented map entry: the handler the generated contract requires, a thin adapter over the rule, and the inputs it reads, declared on the entry's partial request type. Hand-written. |
| `src/Srd52Combat/Initiative`, `Ruleset.cs` | Combatants, the caller's statements, rolls, tie breaks and the order; the replay identity. Hand-written. |
| `src/Srd52Combat/Movement` | Sizes, the grid, and what the caller states about a creature, a square, a terrain feature and a move; the rulings the movement rules answer with. Hand-written. |
| `src/Srd52Combat/Cover`, `Mounts`, `Underwater` | What the caller states about an obstacle between attacker and target, about who is riding what and whether a mount is trained or independent, and about being underwater, having a Swim Speed and what a weapon deals; and the rulings those rules answer with. Hand-written. |
| `src/Srd52Combat/Attacks` | The attack types: what the caller states about what can be seen, what is within 5 feet, whether a roll hit, and how a creature left reach; and what the attack rules answer with. Hand-written. |
| `src/Srd52Combat/Generated`, `tests/Srd52Combat.Tests/Generated` | `MapEntries`, `Registry`, `EntryPoints`, `OwnerRulings`, the request types, the embedded provenance, and the correspondence and provenance tests. Generated by `factory produce`; never edited. |
| `corpus-map.overlay.json` | The only part of the map this engine owns: `status`, `implementedIn` and `tests` on the implemented entries, each test with the mutation that turned it red; and, on five of them, the owner's `rulings` and the `declines` beside them (rules-factory decision 0027). |
| `docs/decisions`, `MAP-FINDINGS.md` | Why the engine reads the map the way it does, and where building it found the map wanting. |
| `MAP-FINDINGS.md` | What building from the map found: where it is wrong, and what is worth recording where it is right. |
| `provenance.json`, `scripts/`, `.github/workflows/validate.yml`, `RulesFactory.Packages.g.props`, `backlog/`, `corpus/` | What the engine was produced from, the gate and its CI, the pins, the backlog, and the pinned corpus. Generated. |

## Rolling Initiative

```csharp
var rolls = EntryPoints.InitiativeRoll.Resolve(new InitiativeRollRequest
{
    Participants = [new("Aria", CombatantKind.PlayerCharacter, 3, D20Mode.Straight),
                    new("Goblin", CombatantKind.Monster, 2, D20Mode.Straight)],
    StatedBy = "the GM",
    ScoreOption = InitiativeScoreOptionStatement.Rolling("the GM"),
    IdenticalCreatures = IdenticalCreaturesStatement.None("the GM"),
    Source = Pcg32.FromSeed(seed, stream: 1),
});
// Resolved: InitiativeRolls, one d20 per participant, Authority srd-5.2.1 / Combat / Initiative / p. 13

// counts: every combatant's InitiativeCount, e.g. InitiativeRolls.Counts
var order = EntryPoints.InitiativeOrder.Resolve(
    new InitiativeOrderRequest(RuleRequest.Empty.Assert("initiative-ties",
        TieBreaks.Of(TieBreak.Ordered(TieDecider.Gm, "the GM", "Goblin", "Aria"))))
    { Counts = counts });
```

| Asked | Answers | Cites |
|---|---|---|
| Roll, every statement allowing one d20 each | the rolls, drawn from the seeded source in participant order | `Combat / Initiative / p. 13` |
| Roll, the GM uses Initiative scores | `OutsideCurrentScope`, nothing drawn, the statement recorded | `Rules Glossary / Initiative / p. 184` (`initiative-score-option`) |
| Roll, a group of identical creatures stated | one d20 each all the same, the statement recorded, naming the owner's ruling `group-initiative/no-grouping` | `Combat / Initiative / p. 13` (`group-initiative`) |
| Roll, a roll with Advantage, Disadvantage, or both (from any source: Surprise, Incapacitated, Invisible) | `OutsideCurrentScope`, nothing drawn | `Playing the Game / Advantage/Disadvantage / p. 7` |
| Roll, a statement missing | `ArgumentException`: never inferred | |
| Order | highest to lowest, the same every round, each tie as its tie break states | `Combat / Initiative / p. 13` |
| Order or ties, a tie and no tie break | `AssertionRequiredException`: never inferred | |
| Order or ties, a tie break by the wrong decider or of other combatants | `ArgumentException`; the decider is checked against the map's `assertedBy` for `initiative-ties` | |
| Who decides a tie | the GM (monsters; monsters and player characters), the players (characters) | `Combat / Initiative / p. 13` |
| Any of them, a monster tied with a non-player character, or players who did not agree | `RequiresInterpretation` | `Combat / Initiative / p. 13` (`initiative-ties-uncovered`) |

See [decision 0001](docs/decisions/0001-initiative-is-rolled-only-where-the-corpus-fixes-the-draws.md)
and [decision 0002](docs/decisions/0002-map-2-0-0-deciders-from-assertedby-and-both-advantage-and-disadvantage-declines.md).

## Moving and the grid

```csharp
// Another creature's space (p. 14): its size, whether it is your ally and whether it has the
// Incapacitated condition are facts the caller states; the engine never infers one.
var ogre = CreatureInSpace.Stranger("an ogre", CreatureSize.Large, statedBy: "the GM");

var terrain = EntryPoints.CreatureSpaceDifficultTerrain.Resolve(
    new CreatureSpaceDifficultTerrainRequest { Other = ogre });
// Resolved: CreatureSpaceTerrain, DifficultTerrainForYou = true, Authority Combat / Moving around
// Other Creatures / p. 14

// The grid is a variant the table opts into: every grid rule demands the statement.
var squares = EntryPoints.GridSpeedInSquares.Resolve(new GridSpeedInSquaresRequest
{
    SpeedInFeet = 30,
    Play = GridPlayStatement.OnAGrid("the GM"),
});
// Resolved: SpeedInSquares, 6 squares (12 feet would be 2, the fraction rounded down)
```

| Asked | Answers | Cites |
|---|---|---|
| A move's modes | one move of those modes, drawing on the one Speed | `Combat / Movement and Position / p. 14` |
| What climbing, crawling, jumping or swimming costs | `OutsideCurrentScope` | `Rules Glossary / Climbing / p. 178` (`movement-modes-glossary`) |
| Dropping Prone, Speed 0 or more | you can, using no action and none of your Speed; you can't at Speed 0 | `Combat / Dropping Prone / p. 14` |
| The size categories | Tiny, Small, Medium, Large, Huge, Gargantuan, smallest first | `Combat / Creature Size / p. 14` |
| Passing through an ally's, an Incapacitated creature's, a Tiny creature's, or a creature exactly two sizes away's space | you may pass through | `Combat / Moving around Other Creatures / p. 14` |
| Passing through any other creature's space | `RequiresInterpretation`: the entry's question is whether "two sizes larger or smaller" is exactly two, and whether every other space is barred | `Combat / Moving around Other Creatures / p. 14` (`moving-through-creatures`) |
| Whether a creature's space is Difficult Terrain | yes, unless it is Tiny or your ally | `Combat / Moving around Other Creatures / p. 14` |
| Ending a move in an occupied space | not willingly; a move the creature was made to take is not forbidden | `Combat / Moving around Other Creatures / p. 14` |
| Ending a turn in a space with another creature | Prone, unless you are Tiny or of a larger size | `Combat / Moving around Other Creatures / p. 14` |
| The square, the Speed in squares, what entering costs, a corner | 5 feet; Speed / 5 rounded down; 1, or 2 for Difficult Terrain, a diagonal step as an orthogonal one; a diagonal step can't cross the corner of a feature that fills its space | `Combat / Playing on a Grid / p. 13` |
| Entering a square held by a Tiny creature or an ally, its terrain not difficult | `RequiresInterpretation`: the rule prices an unoccupied square and a Difficult Terrain one, and that square is neither | `Combat / Playing on a Grid / p. 13` (`grid-entering-square`) |
| Any grid rule, the table not on a grid | `OutsideCurrentScope`, the statement recorded | `Combat / Playing on a Grid / p. 13` (`grid-play`) |
| Any of them, a statement missing | `ArgumentException`: never inferred | |

See [decision 0003](docs/decisions/0003-movement-and-space-the-grid-is-a-variant-the-caller-opts-into.md).

## Making an attack

An attack is three steps, and each is its own entry: the engine performs what the slice states and
names what lies outside it. Nothing here draws: the attack roll is `attack-rolls` (p. 7) and the
damage roll `damage-rolls` (p. 16), both outside this engine's slice.

```csharp
// The structure: the three steps, in order, each naming the entry that holds it.
var structure = EntryPoints.AttackStructure.Resolve(AttackStructureRequest.Empty);

// 1: Choose a Target, at 200 feet with a Longbow (normal 150, long 600).
var target = EntryPoints.AttackTarget.Resolve(new AttackTargetRequest
{
    Kind = TargetKind.Creature,
    Target = "the goblin",
    RangeKind = AttackRangeKind.RangedTwoRanges,
    Ranges = new TwoRanges(150, 600),
    DistanceFeet = 200,
});
// Resolved: ChosenTarget, WithinRange, BeyondNormalRange, the roll has Disadvantage

// 2: Determine Modifiers -- each rule of the slice answers with its own effect.
var unseen = EntryPoints.UnseenTargetDisadvantage.Resolve(
    new UnseenTargetDisadvantageRequest { Visibility = TargetVisibilityStatement.HeardNotSeen("the GM") });

// 3: Resolve the Attack, given the outcome the caller states.
var resolved = EntryPoints.AttackResolution.Resolve(new AttackResolutionRequest
{
    Outcome = AttackRollOutcome.Hits("the GM"),
    DamageRules = AttackDamageRules.Ordinary("the GM"),
});
// Resolved: DamageOnHit, DamageIsRolled, DamageAuthority Damage and Healing / Damage Rolls / p. 16
```

| Asked | Answers | Cites |
|---|---|---|
| What makes an attack, taking the Attack action | an attack is made | `Combat / Making an Attack / p. 14` |
| the same, taking another action, a Bonus Action or a Reaction | `OutsideCurrentScope`; which ones let you attack is elsewhere | `actions-table`, `bonus-actions`, `reactions` |
| the same, while the attacker is Incapacitated | `OutsideCurrentScope` | `Rules Glossary / p. 184` (`incapacitated-condition`) |
| The structure of an attack | the three steps in order, each naming its entry | `Combat / Making an Attack / p. 15` |
| Choose a target, a ranged attack with two ranges | the target, and whether it is within range | `Combat / Making an Attack / p. 15`, `Combat / Range / p. 15` |
| the same, a melee attack | the target, and whether it is within the attacker's reach | `Combat / Melee Attacks / p. 15`, `Combat / Reach / p. 15` |
| the same, a ranged attack with one range | the target, and whether it is within that range | `Combat / Range / p. 15` |
| the same, a kind outside creature, object and location | `ArgumentException`: the set is closed at three | |
| Determine modifiers | the Cover, the Advantage and Disadvantage the caller hands in, and the other effects | `Combat / Making an Attack / p. 15`, `Combat / Cover / p. 15` |
| the same, where `cover-degree` declines | the decline that rule gave, with what it did determine in `Attempted` | `cover-degree`, `cover-no-stacking` |
| Resolve, on a miss | no damage is rolled | `Combat / Making an Attack / p. 15` |
| Resolve, on a hit | damage is rolled, and by which rule | `Combat / Making an Attack / p. 15`, `Damage and Healing / Damage Rolls / p. 16` |
| Resolve, on a hit, the attack's own rules specifying otherwise | no damage is rolled; those rules are the attack's | `Combat / Making an Attack / p. 15` |
| A creature that can't see you | the attack roll has Advantage | `Combat / Unseen Attackers and Targets / p. 14` |
| A target you can't see, heard or only guessed at | the attack roll has Disadvantage | `Combat / Unseen Attackers and Targets / p. 14` |
| A target not in the location you targeted | the attack misses | `Combat / Unseen Attackers and Targets / p. 14` |
| Attacking while hidden, hit or miss | you give away your location | `Combat / Unseen Attackers and Targets / p. 14` |
| Range, at the normal range, beyond it, at the long range, beyond it | none, Disadvantage, Disadvantage, and no attack | `Combat / Range / p. 15` |
| A ranged attack within 5 feet of a seeing, capable enemy | the attack roll has Disadvantage | `Combat / Ranged Attacks in Close Combat / p. 15` |
| A seen creature leaving your reach | take a Reaction for one melee attack with a weapon or an Unarmed Strike, right before it leaves | `Combat / Opportunity Attacks / p. 15` |
| the same, after a Teleport or being moved without its own movement | no Opportunity Attack is provoked | `Combat / Opportunity Attacks / p. 15` |
| the same, after Disengage, moving on that turn | no Opportunity Attack is provoked, naming the engine's decision `disengage-protection-follows-the-glossary` | `Combat / Opportunity Attacks / p. 15`, `disengage-action` |
| the same, having Disengaged on an earlier turn | it provokes: the protection covered that turn | `Combat / Opportunity Attacks / p. 15`, `disengage-action` |
| the same, with the Reaction spent, or while Incapacitated | `OutsideCurrentScope` | `reactions`, `incapacitated-condition` |
| the same, leaving reach by none of the means the glossary lists | `OutsideCurrentScope` | `opportunity-attacks-glossary` |
| Any of them, a statement the caller owes left out | `ArgumentException`: never inferred | |

See [decision 0004](docs/decisions/0004-an-attack-names-what-lies-outside-the-slice-and-performs-what-does-not.md),
[decision 0006](docs/decisions/0006-a-mount-is-a-fact-the-caller-states-and-the-gaps-this-batch-closes.md)
and [MAP-FINDINGS.md](MAP-FINDINGS.md).

## Your turn, and the round

```csharp
// The turn's steps are the caller's, in the order the creature takes them. The corpus's own
// example of breaking up a move: Speed 30, 10 feet, an action, then 20 feet.
var move = EntryPoints.BreakUpMove.Resolve(new BreakUpMoveRequest
{
    Speed = 30,
    Steps = [TurnStep.Moves(10), TurnStep.Acts("the Attack action"), TurnStep.Moves(20)],
    StatedBy = "the players",
});
// Resolved: BrokenMove, 10 feet with 20 left, then 20 feet after the action with 0 left:
// the remainder, never a fresh Speed. Authority Combat / Breaking Up Your Move / p. 14

// Whether the GM requires an action for an activity is the GM's own determination, asserted.
var interactions = EntryPoints.FreeObjectInteraction.Resolve(
    new FreeObjectInteractionRequest(RuleRequest.Empty.Assert("gm-requires-action",
        GmActionRequirement.Of("the GM", RequiredActivity.Interaction("a stuck door"))))
    {
        Interactions = [new ObjectInteraction("a stuck door", InteractionTiming.DuringMove),
                        new ObjectInteraction("the lantern", InteractionTiming.DuringAction)],
    });
// Resolved: TurnInteractions, the stuck door requiring an action (p. 14), the lantern free (p. 13)
```

| Asked | Answers | Cites |
|---|---|---|
| A turn's steps | the distance moved, what is left of the Speed, the action taken, and what (if anything) goes past the turn: a move beyond the Speed, a second action | `Combat / Your Turn / p. 13` |
| A move broken up around an action, a Bonus Action or a Reaction | each part, what it follows, and the movement left after it - the remainder of the one Speed | `Combat / Breaking Up Your Move / p. 14` |
| Forgoing the move, the action, or everything | permitted; nothing is required of the turn, and there is no delay | `Combat / Your Turn / p. 14` |
| The turn's first object interaction, during the move or the action | free | `Combat / Your Turn / p. 13` |
| A second object | the Utilize action | `Combat / Your Turn / p. 13` |
| An interaction the GM requires an action for | an action; it is not the turn's free one | `Combat / Your Turn / p. 14` (`gm-requires-action`) |
| An object whose own description always requires an action | an action; it is not the turn's free one | `Combat / Your Turn / p. 13` |
| Brief or extended communication, as the caller classifies it | free; an action | `Combat / Your Turn / p. 13` |
| Communication the GM's requirement names | `RequiresInterpretation`: whether that requirement reaches communication is not stated | `Combat / Your Turn / p. 13` (`communication-cost`) |
| Either of those two, the GM's requirement not asserted | `AssertionRequiredException`: never inferred | |
| The steps of a combat | establish positions (as stated), roll Initiative, take turns in Initiative order | `Combat / Combat Step by Step / p. 13` |
| The steps, with no Initiative order | `ArgumentException`: no turn is taken before Initiative is rolled | |
| After a round, everyone having acted and neither side defeated | the fight continues to the next round | `Combat / The Order of Combat / p. 13` |
| After a round, a side defeated as stated | no further round; and before everyone has acted, the round is not over | `Combat / The Order of Combat / p. 13` |
| After a round, neither defeated and both sides agreeing to end, as stated | the combat ends and no further round begins, naming the owner's ruling `next-round/agreement-ends-it` | `Combat / The Order of Combat / p. 13` (`next-round`) |
| What surprise gives a combatant stated to be surprised | Disadvantage on their Initiative roll, and nothing else; nothing at all for one that is not surprised | `Combat / Initiative / p. 13` |
| Surprise, while the GM uses Initiative scores | `OutsideCurrentScope`: there is no roll | `Rules Glossary / Initiative / p. 184` (`initiative-score-option`) |
| Who the GM rolls for | the monsters | `Combat / Initiative / p. 13` |
| A group of identical creatures | the statement is recorded and takes no roll of its own; every creature rolls its own Initiative, naming the owner's ruling `group-initiative/no-grouping` | `Combat / Initiative / p. 13` (`group-initiative`) |
| Any of them, a statement the caller owes left out | `ArgumentException`: never inferred | |

See [decision 0005](docs/decisions/0005-the-turn-and-the-round-are-stated-by-the-caller-and-a-group-of-identical-creatures-declines.md)
and [MAP-FINDINGS.md](MAP-FINDINGS.md).

## Cover, reach and the movement budget

```csharp
// The degree of cover one stated obstacle gives: the Cover table's "Offered By" column.
var cover = EntryPoints.CoverDegree.Resolve(new CoverDegreeRequest
{
    Obstacles = [CoveringObstacle.AnObject("the tree trunk", 80, "the GM")],
});
// Resolved: CoverRuling, Three-Quarters Cover, Authority Combat / Cover / p. 15

// Mounting spends the movement budget: half the Speed, rounded down, deducted from what is left.
var deduction = EntryPoints.MovementDeduction.Resolve(new MovementDeductionRequest
{
    SpeedInFeet = 30,
    Parts = [new MovePart("walking to the horse", 10)],
});
// Resolved: MovementSpent, 20 ft left
```

| Asked | Answers | Cites |
|---|---|---|
| The degree an object covering at least half, three-quarters, or the whole target gives | Half, Three-Quarters, Total | `Combat / Cover / p. 15` |
| the same, an object covering less than half, or nothing between attacker and target | no degree: the table's least needs half | `Combat / Cover / p. 15` |
| the same, a creature covering at least half | Half Cover, and never more: the other rows name only an object | `Combat / Cover / p. 15` |
| the same, a creature covering less than half | `RequiresInterpretation`: whether "that covers at least half" qualifies "Another creature" decides this case and the corpus does not choose | `Combat / Cover / p. 15` (`cover-degree`) |
| the same, more than one stated source of cover | `UnsupportedRule`, not built | `cover-no-stacking` |
| A creature's reach | 5 feet, or the greater reach the caller states its description gives it | `Combat / Reach / p. 15` |
| A melee attack's target | whether it is within that reach | `Combat / Melee Attacks / p. 15` |
| A ranged attack with one range | whether the target is within it; beyond it can't be attacked | `Combat / Range / p. 15` |
| How far a creature may move | its Speed or less, nothing at all included | `Combat / Movement and Position / p. 14` |
| What a move's parts cost | each deducted whole, in order, until the Speed is used up; a part the movement left does not cover is not taken, and neither is what follows | `Combat / Movement and Position / p. 14` |

## Mounted combat, and underwater

```csharp
// Who is on what is a fact the caller states (decision 0006); since the owner's ruling of
// 2026-09-16 the GM's determination of the anatomy also reaches mount-eligibility (decision 0007).
var horse = MountStatement.Ridden("the horse", "the GM");

var turn = EntryPoints.ControlledMountTurn.Resolve(new ControlledMountTurnRequest
{
    Creature = MountCreatureStatement.DomesticatedHorse("the horse", "the GM"),
    Mount = horse,
});
// Resolved: ControlledMountTurn, the rider's Initiative, and only Dash, Disengage and Dodge

var impeded = EntryPoints.UnderwaterMelee.Resolve(new UnderwaterMeleeRequest
{
    Where = UnderwaterStatement.Is("the GM"),
    SwimSpeed = SwimSpeedStatement.Lacks("the GM"),
    Weapon = WeaponStatement.NotPiercing("Warhammer", "the GM"),
});
// Resolved: RollDetermination, Disadvantage, Authority Combat / Impeded Weapons / p. 16
```

| Asked | Answers | Cites |
|---|---|---|
| Can this creature be a mount: unwilling, or not at least one size larger | no | `Combat / Mounted Combat / p. 15` |
| the same, willing, large enough, and an anatomy the GM has determined | what the GM determined, naming the owner's ruling `appropriate-anatomy/gm-decides` | `Combat / Mounted Combat / p. 15` (`appropriate-anatomy`) |
| the same, with the GM having determined nothing | `RequiresInterpretation`: the engine never assumes an anatomy | `Combat / Mounted Combat / p. 15` (`appropriate-anatomy`) |
| What anatomy is appropriate | the GM's determination, naming the owner's ruling; `RequiresInterpretation` where the GM has made none | `Combat / Mounted Combat / p. 15` (`appropriate-anatomy`) |
| Mounting or dismounting | half the Speed, rounded down, deducted from the movement left | `Combat / Mounting and Dismounting / p. 15` |
| the same, less movement left than the cost, or a creature more than 5 feet away | it is not done | `Combat / Mounting and Dismounting / p. 15` |
| Can the mount be controlled: a domesticated horse or a mule | yes: the corpus names them as trained | `Combat / Controlling a Mount / p. 16` |
| the same, any other creature whose training the caller states | what the caller stated, naming the owner's ruling `mount-control-requires-training/training-is-stated` | `Combat / Controlling a Mount / p. 16` (`mount-control-requires-training`) |
| the same, any other creature whose training nothing states | `RequiresInterpretation`: "similar creatures" states no measure, and the engine is not told | `Combat / Controlling a Mount / p. 16` (`mount-control-requires-training`) |
| A controlled mount's turn | the rider's Initiative, moving on the rider's turn, and only Dash, Disengage and Dodge; it acts on the turn it is mounted | `Combat / Controlling a Mount / p. 16` |
| the same, what one of those three actions does | `OutsideCurrentScope` | `actions-table` |
| A mount that ignores the rider's control | it is independent: it keeps its place in the Initiative order and moves and acts as it likes | `Combat / Controlling a Mount / p. 16` |
| the same, nothing stated about control, or what it does with its turn | `RequiresInterpretation`: the corpus names neither what makes a mount independent nor a decider for its choices | `Combat / Controlling a Mount / p. 16` (`independent-mount`) |
| Falling off: moved against the mount's will, the rider knocked Prone, or the mount | a DC 10 Dexterity saving throw, made under `saving-throws` | `Combat / Falling Off / p. 16` |
| the same, the save succeeded | the rider stays on | `Combat / Falling Off / p. 16` |
| the same, the save failed | `RequiresInterpretation`: the rider falls off Prone, and which unoccupied space within 5 feet is not stated | `Combat / Falling Off / p. 16` (`falling-off`) |
| Any mounted-combat rule, nothing stating a mount or a rider | `OutsideCurrentScope`, the statement recorded | `mount-eligibility`, `mounting-cost` |
| A melee weapon attack underwater, no Swim Speed, not Piercing | the attack roll has Disadvantage | `Combat / Impeded Weapons / p. 16` |
| the same, with a Swim Speed or with a Piercing weapon | nothing | `Combat / Impeded Weapons / p. 16` |
| A ranged weapon attack underwater, within normal range | Disadvantage | `Combat / Impeded Weapons / p. 16` |
| the same, beyond normal range | it automatically misses | `Combat / Impeded Weapons / p. 16` |
| Anything underwater, and Fire damage | it has Resistance, which is explained in `resistance` | `Combat / Fire Resistance / p. 16` |
| Any of them, a statement the caller owes left out | `ArgumentException`: never inferred | |

## Randomness and replay

The corpus declares `randomness: seeded`. Every draw goes through `RulesKernel.Randomness`
(`IRandomSource`, `UniformInt`) from a source the caller seeds, and `Ruleset.Identity` names PCG32.
`SeededInitiativeReplayTests` rolls a seven-combatant start with three ties through the entry points,
replays it from the seed and the recorded tie breaks, compares the bytes, and pins their SHA-256. The
bytes name the ruleset (`srd-5.2.1-combat` v7), the replay schema (2, which is the record's
`rulings` line: the owner's rulings its answers relied on) and the map version (2.0.0). Only Initiative draws:
nothing in movement, the grid, the attack entries, the turn and round entries, or the mounted-combat,
underwater and cover entries does (decisions 0003, 0004, 0005 and 0006), so none of them enters the
replay. `group-initiative` is the one entry whose rule would draw, and under the owner's ruling it draws
nothing: every participant's d20 is `initiative-roll`'s, whatever the caller states about identical
creatures; `falling-off` carries a `draws` in the map and draws nothing
here, because the saving throw is `saving-throws`', outside the extent (MAP-FINDINGS finding 18).

## How this engine is produced

From a clean rules-factory checkout, with the SDK `global.json` pins:

```bash
python3 tools/factory produce --package RulesFactory.Maps.Srd52Combat@2.0.0 \
  --corpus <this repository>/corpus/srd-5.2.1.txt --name Srd52Combat --out <this repository>
```

`provenance.json` records the run: rules-factory 0.7.0 (tag `factory/v0.7.0`, commit `0697808`,
clean). After changing only the overlay, run `produce` again too: the generated correspondence
tests read the merged statuses, and `provenance.json` hashes the overlay. To check the record against the tree, from a rules-factory checkout at that tag:

```bash
python3 tools/factory provenance --engine <this repository>
```

## Verify it

```bash
./scripts/validate.sh full
```

## Licence

This repository is under two sets of terms; `NOTICE` says which files are under which.

- **The engine** (code, tests, scripts, documentation, and everything that is not SRD text) is
  licensed under the Apache License 2.0 (`LICENSE`).
- **SRD 5.2.1 text** (`corpus/srd-5.2.1.txt`, and the verbatim quotations of it in `backlog/` and
  elsewhere) is licensed under CC-BY-4.0, not Apache-2.0. The corpus is the text pdftotext 24.02.0
  extracts from the PDF, with a page marker line before each page; quotations are verbatim excerpts
  of it.

The attribution statement the SRD 5.2.1 requires, verbatim:

> This work includes material from the System Reference Document 5.2.1 (“SRD 5.2.1”) by Wizards of the Coast LLC, available at https://www.dndbeyond.com/srd. The SRD 5.2.1 is licensed under the Creative Commons Attribution 4.0 International License, available at https://creativecommons.org/licenses/by/4.0/legalcode.
