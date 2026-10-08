#!/usr/bin/env python3
# /// script
# requires-python = ">=3.12"
# ///
"""
Build docs/meme-precept-catalog.md: every meme and every precept issue found in the
base game, the DLCs, this mod, and the locally installed mods, grouped by the mod that
defines them, with the PreceptPolicy category of each issue.

    uv run scripts/meme_precept_catalog.py
"""

from __future__ import annotations

import argparse
import re
import sys
import xml.etree.ElementTree as ET
from collections import defaultdict
from dataclasses import dataclass, field
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
GAME = Path.home() / ".local/share/Steam/steamapps/common/RimWorld"
WORKSHOP = Path.home() / ".local/share/Steam/steamapps/workshop/content/294100"
MODS_CONFIG = (
    Path.home() / ".config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Config/ModsConfig.xml"
)
POLICY_FILES = [REPO / "Source/Precepts/PreceptPolicy.cs"]
GAME_VERSION = "1.6"
DLC_ORDER = ["Core", "Royalty", "Ideology", "Biotech", "Anomaly", "Odyssey"]
SKIPPED_DIRS = {"About", "Languages", "Patches", "Source", "Textures", "Sounds", ".git", ".vs"}
VERSION_DIR = re.compile(r"^v?\d+\.\d+$")
DEF_TAGS = {"MemeDef", "PreceptDef", "IssueDef"}
LIST_ISSUES = {"Ritual", "IdeoRole", "IdeoBuilding", "IdeoRitualSeat", "IdeoRelic", "Weapons"}
DESCRIPTION_LIMIT = 140


@dataclass
class Mod:
    name: str
    package_id: str
    root: Path
    active: bool
    order: int


@dataclass
class Def:
    tag: str
    def_name: str
    mod: Mod
    fields: dict[str, ET.Element]

    def text(self, key: str) -> str:
        elem = self.fields.get(key)
        return (elem.text or "").strip() if elem is not None else ""

    def items(self, key: str) -> list[str]:
        elem = self.fields.get(key)
        if elem is None:
            return []
        return [(li.text or "").strip() for li in elem.iter("li") if li.text and li.text.strip()]


@dataclass
class Catalog:
    memes: dict[str, Def] = field(default_factory=dict)
    precepts: dict[str, Def] = field(default_factory=dict)
    issues: dict[str, Def] = field(default_factory=dict)


def read_active_ids() -> list[str]:
    root = ET.parse(MODS_CONFIG).getroot()
    return [li.text.strip().lower() for li in root.find("activeMods").iter("li") if li.text]


def read_about(root: Path) -> tuple[str, str] | None:
    about = root / "About" / "About.xml"
    if not about.is_file():
        return None
    try:
        meta = ET.parse(about).getroot()
    except ET.ParseError:
        return None
    package_id = (meta.findtext("packageId") or "").strip().lower()
    name = (meta.findtext("name") or root.name).strip()
    return (name, package_id) if package_id else None


def discover_mods() -> list[Mod]:
    active_ids = read_active_ids()
    rank = {pid: ii for ii, pid in enumerate(active_ids)}
    mods: dict[str, Mod] = {}

    def add(root: Path, name: str, package_id: str) -> None:
        if package_id in mods:
            return
        active = package_id in rank
        order = rank.get(package_id, len(rank) + len(mods))
        mods[package_id] = Mod(name, package_id, root, active, order)

    for dlc in DLC_ORDER:
        root = GAME / "Data" / dlc
        if root.is_dir():
            package_id = f"ludeon.rimworld.{dlc.lower()}" if dlc != "Core" else "ludeon.rimworld"
            add(root, dlc if dlc == "Core" else f"{dlc} (DLC)", package_id)
    about = read_about(REPO)
    if about:
        add(REPO, *about)
    for parent in (GAME / "Mods", WORKSHOP):
        for root in sorted(parent.iterdir()):
            about = read_about(root)
            if about:
                add(root, *about)
    return sorted(mods.values(), key=lambda mod: mod.order)


def def_files(mod: Mod):
    for path in mod.root.rglob("*.xml"):
        parts = path.relative_to(mod.root).parts[:-1]
        if not path.is_file() or "Defs" not in parts:
            continue
        if any(part in SKIPPED_DIRS for part in parts):
            continue
        if any(VERSION_DIR.match(part) and part.lstrip("v") != GAME_VERSION for part in parts):
            continue
        yield path


def parse_defs(mods: list[Mod]) -> tuple[list[tuple[ET.Element, Mod]], dict[tuple[str, str], ET.Element]]:
    raw: list[tuple[ET.Element, Mod]] = []
    named: dict[tuple[str, str], ET.Element] = {}
    for mod in mods:
        for path in def_files(mod):
            try:
                root = ET.parse(path).getroot()
            except ET.ParseError as err:
                print(f"skip {path}: {err}", file=sys.stderr)
                continue
            if root.tag != "Defs":
                continue
            for elem in root:
                if elem.tag not in DEF_TAGS:
                    continue
                if name := elem.get("Name"):
                    named.setdefault((elem.tag, name), elem)
                raw.append((elem, mod))
    return raw, named


def resolve_fields(
    elem: ET.Element, named: dict[tuple[str, str], ET.Element], seen: frozenset[str] = frozenset()
) -> dict[str, ET.Element]:
    fields: dict[str, ET.Element] = {}
    parent_name = elem.get("ParentName")
    if parent_name and parent_name not in seen:
        parent = named.get((elem.tag, parent_name))
        if parent is not None:
            fields.update(resolve_fields(parent, named, seen | {parent_name}))
    fields.update({child.tag: child for child in elem})
    return fields


def build_catalog(mods: list[Mod]) -> Catalog:
    raw, named = parse_defs(mods)
    catalog = Catalog()
    by_tag = {"MemeDef": catalog.memes, "PreceptDef": catalog.precepts, "IssueDef": catalog.issues}
    for elem, mod in raw:
        if elem.get("Abstract", "").lower() == "true":
            continue
        fields = resolve_fields(elem, named)
        def_name_elem = fields.get("defName")
        if def_name_elem is None or not def_name_elem.text:
            continue
        def_name = def_name_elem.text.strip()
        by_tag[elem.tag].setdefault(def_name, Def(elem.tag, def_name, mod, fields))
    return catalog


def read_policy() -> dict[str, str]:
    source = "\n".join(path.read_text() for path in POLICY_FILES)
    sets = {
        "MoralIssues": "Moral",
        "UniversalPositiveIssues": "UniversalPositive",
        "SpecialIssues": "Special",
        "NAIssues": "NA",
        "KnownPositiveOnlyIssues": "PositiveOnly",
    }
    policy: dict[str, str] = {}
    for set_name, category in sets.items():
        match = re.search(rf"HashSet<string> {set_name}\s*=\s*\[(.*?)\];", source, re.S)
        body = re.sub(r"//[^\n]*", "", match.group(1))
        for issue in re.findall(r'"([^"]+)"', body):
            policy.setdefault(issue, category)
    return policy


def first_sentence(text: str) -> str:
    text = " ".join(text.replace("\\n", " ").split())
    sentence = re.split(r"(?<=[.!?])\s", text, maxsplit=1)[0]
    if len(sentence) > DESCRIPTION_LIMIT:
        sentence = sentence[: DESCRIPTION_LIMIT - 1].rstrip() + "…"
    return sentence.replace("|", "\\|")


def short_mod(mod: Mod) -> str:
    name = mod.name.removesuffix(" (DLC)")
    words = re.findall(r"[A-Za-z0-9]+", name)
    return name if len(words) == 1 else "".join(word[0].upper() for word in words)


def meme_links(catalog: Catalog) -> tuple[dict[str, set[str]], dict[str, set[str]]]:
    """Return meme -> linked issues and issue -> linked memes, through any precept rung."""
    meme_issues: dict[str, set[str]] = defaultdict(set)
    issue_memes: dict[str, set[str]] = defaultdict(set)

    def link(meme: str, precept: str) -> None:
        rung = catalog.precepts.get(precept)
        if rung is None or meme not in catalog.memes:
            return
        issue = rung.text("issue")
        meme_issues[meme].add(issue)
        issue_memes[issue].add(meme)

    for precept in catalog.precepts.values():
        for meme in precept.items("requiredMemes") + precept.items("associatedMemes"):
            link(meme, precept.def_name)
    for meme in catalog.memes.values():
        for precept in meme.items("requireOne"):
            link(meme.def_name, precept)
        select = meme.fields.get("selectOneOrNone")
        if select is not None:
            for precept in select.iter("precept"):
                link(meme.def_name, (precept.text or "").strip())
    return meme_issues, issue_memes


def rung_order(rung: Def) -> float:
    try:
        return float(rung.text("displayOrderInIssue") or 0)
    except ValueError:
        return 0.0


def mod_heading(mod: Mod) -> str:
    state = "" if mod.active else ", inactive"
    return f"### {mod.name} (`{mod.package_id}`{state})"


def render_issues(catalog: Catalog, policy: dict[str, str], issue_memes: dict[str, set[str]]) -> list[str]:
    rungs_by_issue: dict[str, list[Def]] = defaultdict(list)
    for precept in catalog.precepts.values():
        if precept.text("classic").lower() != "true":
            rungs_by_issue[precept.text("issue")].append(precept)
    tagged_mods: dict[str, Mod] = {}
    by_mod: dict[str, list[Def]] = defaultdict(list)
    for issue in catalog.issues.values():
        if rungs_by_issue.get(issue.def_name):
            by_mod[issue.mod.package_id].append(issue)

    lines = ["## Precept issues by mod", ""]
    for mod in sorted({issue.mod.package_id: issue.mod for issue in catalog.issues.values()}.values(),
                      key=lambda mod: mod.order):
        issues = sorted(by_mod.get(mod.package_id, []), key=lambda issue: issue.def_name.lower())
        if not issues:
            continue
        lines += [mod_heading(mod), "",
                  "| Issue | Label | Category | Rungs | Linked memes |",
                  "|---|---|---|---|---|"]
        for issue in issues:
            rungs = sorted(rungs_by_issue[issue.def_name], key=lambda rung: (rung_order(rung), rung.def_name))
            category = policy.get(issue.def_name, "PositiveOnly (default)")
            if issue.def_name in LIST_ISSUES:
                rung_text = f"*{len(rungs)} precepts*"
            else:
                tagged_mods.update({short_mod(rung.mod): rung.mod for rung in rungs if rung.mod is not mod})
                rung_text = ", ".join(
                    (rung.text("label") or rung.def_name)
                    + ("" if rung.mod is mod else f" [{short_mod(rung.mod)}]")
                    for rung in rungs
                )
            memes = ", ".join(sorted(issue_memes.get(issue.def_name, ())))
            lines.append(f"| {issue.def_name} | {issue.text('label')} | {category} | {rung_text} | {memes} |")
        lines.append("")

    lines += ["### Rung tags", "", "| Tag | Mod |", "|---|---|"]
    lines += [f"| {tag} | {mod.name} |" for tag, mod in sorted(tagged_mods.items())]
    lines.append("")

    missing = sorted(name for name in policy if name not in catalog.issues)
    if missing:
        lines += ["### Classified in PreceptPolicy but not installed locally", "",
                  ", ".join(f"`{name} ({policy[name]})`" for name in missing), ""]
    return lines


def render_memes(catalog: Catalog, policy: dict[str, str], meme_issues: dict[str, set[str]]) -> list[str]:
    by_mod: dict[str, list[Def]] = defaultdict(list)
    for meme in catalog.memes.values():
        by_mod[meme.mod.package_id].append(meme)
    mods = sorted({meme.mod.package_id: meme.mod for meme in catalog.memes.values()}.values(),
                  key=lambda mod: mod.order)

    lines = ["## Memes by mod", ""]
    for mod in mods:
        memes = sorted(by_mod[mod.package_id],
                       key=lambda meme: (meme.text("category") == "Structure", meme.def_name.lower()))
        lines += [mod_heading(mod), "",
                  "| Meme | Label | Kind | Linked issues | Agreeable | Disagreeable | Description |",
                  "|---|---|---|---|---|---|---|"]
        for meme in memes:
            issues = sorted(meme_issues.get(meme.def_name, ()), key=str.lower)
            issue_text = ", ".join(
                f"**{issue}**" if policy.get(issue) in ("Moral", "Special") else issue for issue in issues
            )
            kind = "structure" if meme.text("category") == "Structure" else "meme"
            agreeable = ", ".join(trait_names(meme, "agreeableTraits"))
            disagreeable = ", ".join(trait_names(meme, "disagreeableTraits"))
            lines.append(
                f"| {meme.def_name} | {meme.text('label')} | {kind} | {issue_text} | {agreeable} | "
                f"{disagreeable} | {first_sentence(meme.text('description'))} |"
            )
        lines.append("")
    return lines


def trait_names(meme: Def, key: str) -> list[str]:
    elem = meme.fields.get(key)
    if elem is None:
        return []
    names = []
    for li in elem.findall("li"):
        trait = (li.findtext("def") or li.text or "").strip()
        degree = (li.findtext("degree") or "").strip()
        if trait:
            names.append(f"{trait} ({degree})" if degree else trait)
    return names


def align_tables(lines: list[str]) -> list[str]:
    """Pad every markdown table to fixed column widths, with an org-style separator row."""
    out: list[str] = []
    table: list[list[str]] = []

    def flush() -> None:
        if not table:
            return
        body = [row for ii, row in enumerate(table) if ii != 1]
        widths = [max(len(row[cc]) for row in body) for cc in range(len(body[0]))]
        for ii, row in enumerate(table):
            if ii == 1:
                out.append("|" + "+".join("-" * (width + 2) for width in widths) + "|")
            else:
                out.append("| " + " | ".join(cell.ljust(width) for cell, width in zip(row, widths)) + " |")
        table.clear()

    for line in lines:
        if line.startswith("|"):
            table.append([cell.strip() for cell in re.split(r"(?<!\\)\|", line.strip())[1:-1]])
        else:
            flush()
            out.append(line)
    flush()
    return out


def render(catalog: Catalog, policy: dict[str, str]) -> str:
    meme_issues, issue_memes = meme_links(catalog)
    header = [
        "# Meme and precept catalogue",
        "",
        "Generated by `uv run scripts/meme_precept_catalog.py`; do not edit by hand.",
        "It lists every meme and every precept issue in the base game, the DLCs, this mod, and the mods installed locally (active or not), grouped by the mod that defines the def.",
        "Use it with [trait-meme-affinity.md](trait-meme-affinity.md) to choose trait–meme–precept associations.",
        "",
        "- **Category** comes from `PreceptPolicy` (see [preceptPolicy.md](preceptPolicy.md)).",
        "  Only Moral and Special issues are debatable and feed structural opinion; issues not listed there default to PositiveOnly.",
        "- **Rungs** are in `displayOrderInIssue` order, before any `OrderOverrides`.",
        "  A rung that another mod adds carries that mod's tag in brackets (see \"Rung tags\" at the end).",
        "  Classic-mode precepts are left out.",
        "- **Linked memes / issues** come from `requiredMemes`, `associatedMemes` and the meme's `requireOne` and `selectOneOrNone`, the same links that trait–meme conviction seeding follows.",
        "  In the meme tables, debatable issues are in bold.",
        "- **Agreeable / Disagreeable** are the traits in the meme def itself; trait-mod patches are not applied (see trait-meme-affinity.md for those).",
        "- Only defs under the root, `Common/`, or `1.6/` folders are read, so mods without 1.6 content do not appear.",
        "",
    ]
    return "\n".join(align_tables(header + render_memes(catalog, policy, meme_issues)
                                  + render_issues(catalog, policy, issue_memes)))


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--output", type=Path, default=REPO / "docs" / "meme-precept-catalog.md")
    args = parser.parse_args()
    mods = discover_mods()
    catalog = build_catalog(mods)
    args.output.write_text(render(catalog, read_policy()))
    print(f"{len(catalog.memes)} memes, {len(catalog.issues)} issues, "
          f"{len(catalog.precepts)} precepts -> {args.output}")


if __name__ == "__main__":
    main()
