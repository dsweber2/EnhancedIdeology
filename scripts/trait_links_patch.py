#!/usr/bin/env python3
# /// script
# requires-python = ">=3.12"
# ///
"""
Build Common/Patches/TraitLinks.xml from the `meme/precepts` column of
docs/trait-meme-affinity.md ("Traits with no meme affinity" tables).

    uv run scripts/trait_links_patch.py

Cell grammar (see the legend in the doc):
    Meme (+) / Meme (-)       trait goes in the meme's agreeableTraits / disagreeableTraits
    [Issue → Suffix]          trait links to the rung whose defName ends in _Suffix (TraitIssueLinks)
    [Issue] (-)               trait opposes a UniversalPositive issue (TraitIssueLinks)
    deg1/deg2: items; ...     items apply only to the trait degrees with those labels
Each operation checks that the trait and meme exist, so links to absent mods are skipped.
"""

from __future__ import annotations

import argparse
import re
import sys
from collections import defaultdict
from dataclasses import dataclass, field
from pathlib import Path
from xml.sax.saxutils import escape

sys.path.insert(0, str(Path(__file__).resolve().parent))
import meme_precept_catalog as catalog_lib  # noqa: E402

REPO = Path(__file__).resolve().parent.parent
DOC = REPO / "docs" / "trait-meme-affinity.md"
OUTPUT = REPO / "Common" / "Patches" / "TraitLinks.xml"
SECTION = "## Traits with no meme affinity"
MEME_ITEM = re.compile(r"^(\w+) \(([+-])\)$")
RUNG_ITEM = re.compile(r"^\[(\w+) → (\w+)\]$")
OPPOSE_ITEM = re.compile(r"^\[(\w+)\] \(-\)$")
DEGREE_LABEL = re.compile(r"(.+?) \((-?\d+)\)")


@dataclass
class TraitLinks:
    memes: dict[tuple[str, str], list[int | None]] = field(default_factory=lambda: defaultdict(list))
    rungs: list[tuple[str, int | None]] = field(default_factory=list)
    opposes: list[tuple[str, int | None]] = field(default_factory=list)


def table_rows(lines: list[str]):
    """Yield (trait, cell, label) for every row of every table in the section."""
    start = lines.index(SECTION)
    columns: list[str] = []
    for line in lines[start:]:
        if not line.startswith("|"):
            columns = []
            continue
        if line.startswith("|-"):
            continue
        cells = [cell.strip() for cell in line.strip().strip("|").split("|")]
        if not columns:
            columns = cells
            continue
        row = dict(zip(columns, cells))
        yield row["Trait"], row.get("meme/precepts", ""), row["Label"]


def degree_map(label: str) -> dict[str, int]:
    matches = (DEGREE_LABEL.fullmatch(part.strip()) for part in label.split(","))
    return {match.group(1): int(match.group(2)) for match in matches if match}


def resolve_degrees(prefix: str, degrees: dict[str, int], trait: str) -> list[int | None]:
    resolved: list[int | None] = []
    for token in (token.strip() for token in prefix.split("/")):
        exact = [degree for name, degree in degrees.items() if name == token]
        loose = [degree for name, degree in degrees.items()
                 if name.startswith(token + " ") or name.endswith(" " + token)]
        found = exact or loose
        if len(found) != 1:
            raise ValueError(f"{trait}: degree label '{token}' matches {len(found)} degrees")
        resolved.append(found[0])
    return resolved


def resolve_rung(issue: str, suffix: str, precepts: dict) -> str:
    candidates = [name for name, precept in precepts.items()
                  if precept.text("issue") == issue and name.endswith("_" + suffix)]
    exact = f"{issue}_{suffix}"
    if exact in candidates or not candidates:
        if not candidates:
            print(f"warning: rung {exact} not found locally; using it as written", file=sys.stderr)
        return exact
    if len(candidates) > 1:
        raise ValueError(f"[{issue} → {suffix}] matches {candidates}")
    return candidates[0]


def parse(lines: list[str], catalog) -> tuple[dict[str, TraitLinks], list[str]]:
    links: dict[str, TraitLinks] = {}
    problems: list[str] = []
    for trait, cell, label in table_rows(lines):
        if not cell or not re.fullmatch(r"\w+", trait):
            continue
        entry = links.setdefault(trait, TraitLinks())
        for segment in (segment.strip() for segment in cell.split(";")):
            prefix, _, items = segment.rpartition(":")
            degrees = resolve_degrees(prefix, degree_map(label), trait) if prefix else [None]
            for item in (item.strip() for item in items.split(",")):
                if match := MEME_ITEM.match(item):
                    meme, sign = match.groups()
                    if meme not in catalog.memes:
                        problems.append(f"{trait}: meme {meme} not found locally")
                    kind = "agreeableTraits" if sign == "+" else "disagreeableTraits"
                    entry.memes[(meme, kind)].extend(degrees)
                elif match := RUNG_ITEM.match(item):
                    rung = resolve_rung(*match.groups(), catalog.precepts)
                    entry.rungs.extend((rung, degree) for degree in degrees)
                elif match := OPPOSE_ITEM.match(item):
                    entry.opposes.extend((match.group(1), degree) for degree in degrees)
                else:
                    problems.append(f"{trait}: cannot parse '{item}'")
    return links, problems


def trait_items(trait: str, degrees: list[int | None]) -> str:
    if None in degrees:
        return f"<li>{trait}</li>"
    return "".join(f"<{trait}>{degree}</{trait}>" for degree in sorted(set(degrees)))


def meme_operation(meme: str, kind: str, items: str) -> list[str]:
    meme_path = f'/Defs/MemeDef[defName="{meme}"]'
    return [
        '\t\t\t\t<li Class="PatchOperationConditional">',
        f"\t\t\t\t\t<xpath>{meme_path}/{kind}</xpath>",
        '\t\t\t\t\t<match Class="PatchOperationAdd">',
        f"\t\t\t\t\t\t<xpath>{meme_path}/{kind}</xpath>",
        f"\t\t\t\t\t\t<value>{items}</value>",
        "\t\t\t\t\t</match>",
        '\t\t\t\t\t<nomatch Class="PatchOperationConditional">',
        f"\t\t\t\t\t\t<xpath>{meme_path}</xpath>",
        '\t\t\t\t\t\t<match Class="PatchOperationAdd">',
        f"\t\t\t\t\t\t\t<xpath>{meme_path}</xpath>",
        f"\t\t\t\t\t\t\t<value><{kind}>{items}</{kind}></value>",
        "\t\t\t\t\t\t</match>",
        "\t\t\t\t\t</nomatch>",
        "\t\t\t\t</li>",
    ]


def link_li(field_name: str, value: str, degree: int | None) -> str:
    degree_xml = f"<degree>{degree}</degree>" if degree is not None else ""
    return f"<li>{degree_xml}<{field_name}>{escape(value)}</{field_name}></li>"


def extension_operation(trait: str, entry: TraitLinks) -> list[str]:
    items = [link_li("rung", rung, degree) for rung, degree in entry.rungs]
    items += [link_li("opposes", issue, degree) for issue, degree in entry.opposes]
    return [
        '\t\t\t\t<li Class="PatchOperationAddModExtension">',
        f'\t\t\t\t\t<xpath>/Defs/TraitDef[defName="{trait}"]</xpath>',
        "\t\t\t\t\t<value>",
        '\t\t\t\t\t\t<li Class="EnhancedIdeology.TraitIssueLinks">',
        "\t\t\t\t\t\t\t<links>",
        *(f"\t\t\t\t\t\t\t\t{item}" for item in items),
        "\t\t\t\t\t\t\t</links>",
        "\t\t\t\t\t\t</li>",
        "\t\t\t\t\t</value>",
        "\t\t\t\t</li>",
    ]


def render(links: dict[str, TraitLinks]) -> str:
    out = [
        '<?xml version="1.0" encoding="utf-8"?>',
        "<!-- Generated by scripts/trait_links_patch.py from docs/trait-meme-affinity.md. Do not edit. -->",
        "<Patch>",
    ]
    for trait in sorted(links):
        entry = links[trait]
        operations: list[str] = []
        for (meme, kind), degrees in sorted(entry.memes.items()):
            operations += meme_operation(meme, kind, trait_items(trait, degrees))
        if entry.rungs or entry.opposes:
            operations += extension_operation(trait, entry)
        if not operations:
            continue
        out += [
            "",
            f"\t<!-- {trait} -->",
            '\t<Operation Class="PatchOperationConditional">',
            f'\t\t<xpath>/Defs/TraitDef[defName="{trait}"]</xpath>',
            '\t\t<match Class="PatchOperationSequence">',
            "\t\t\t<operations>",
            *operations,
            "\t\t\t</operations>",
            "\t\t</match>",
            "\t</Operation>",
        ]
    out += ["", "</Patch>", ""]
    return "\n".join(out)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--output", type=Path, default=OUTPUT)
    args = parser.parse_args()
    catalog = catalog_lib.build_catalog(catalog_lib.discover_mods())
    links, problems = parse(DOC.read_text().split("\n"), catalog)
    for problem in problems:
        print(f"warning: {problem}", file=sys.stderr)
    args.output.write_text(render(links))
    print(f"{sum(1 for entry in links.values() if entry.memes or entry.rungs or entry.opposes)} traits -> {args.output}")


if __name__ == "__main__":
    main()
