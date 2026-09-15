# SRD 5.2.1 Combat

A deterministic rules engine for the Combat chapter of the System Reference Document 5.2.1
(pp. 13–16), produced by [rules-factory](https://github.com/brandonifco/rules-factory) from a
corpus map, on [`RulesKernel`](https://www.nuget.org/packages/RulesKernel) 0.2.0.

The corpus is the SRD 5.2.1 as text extracted from the official PDF, pinned in
`corpus/srd-5.2.1.txt` and hashed on every validation run. The specification is the map in the
package [`RulesFactory.Maps.Srd52Combat`](https://www.nuget.org/packages/RulesFactory.Maps.Srd52Combat)
1.0.0: ninety-one entries, seventy in scope.

Citations are by heading path and printed page (`Combat / Initiative / p. 13`). The corpus declares
`randomness: seeded` (rules-factory decision 0019): the engine may draw random values, only through
`RulesKernel.Randomness`'s seeded PCG32 source, so that a combat replays from its seed.

## State

Produced, with no rule implemented yet: every in-scope entry declines through its generated entry
point with the reason the map's correspondence table gives and its own citation. The rules still
to build are listed in `backlog/`.

## How this engine is produced

From a clean rules-factory checkout, with the SDK `global.json` pins:

```bash
python3 tools/factory produce --package RulesFactory.Maps.Srd52Combat@1.0.0 \
  --corpus <this repository>/corpus/srd-5.2.1.txt --name Srd52Combat --out <this repository>
```

`provenance.json` records the run: rules-factory 0.3.1 (tag `factory/v0.3.1`, commit `933c3a6`,
clean). To check the record against the tree, from a rules-factory checkout at that tag:

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
