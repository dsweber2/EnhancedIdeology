#!/usr/bin/env python3
# /// script
# requires-python = ">=3.12"
# ///
"""
Translation helper for EnhancedIdeology.

    # Generate a blank German template:
    uv run scripts/translate.py extract --source English --target German

    # Pre-fill template from existing partial translation:
    uv run scripts/translate.py extract --source English --target German --prefill

    # Write XML files from a filled-in template:
    uv run scripts/translate.py generate --input translation_German.md
"""

from __future__ import annotations

import argparse
import re
import sys
import xml.etree.ElementTree as ET
from collections import defaultdict
from dataclasses import dataclass
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
DEFS_DIR = ROOT / "Common" / "Defs"
LANGS_DIR = ROOT / "Common" / "Languages"
TRANSLATIONS_DIR = ROOT / "translations"

# Tags whose text content is directly translatable.
LEAF_FIELDS: frozenset[str] = frozenset({
    "label", "description", "jobString", "reportString",
    "beginLetter", "beginLetterLabel", "endMessage", "baseInspectLine",
    "recoveryMessage", "effectDesc", "letterInfoText", "verb", "gerund",
})

# Tags we should not recurse into (non-content containers).
SKIP_TAGS: frozenset[str] = frozenset({
    "graphicData", "statBases", "building", "stuffCategories",
    "thingCategories", "inspectorTabs", "placeWorkers", "mote",
    "ingredients", "fixedIngredientFilter", "products", "enablesNeeds",
    "nullifyingHediffs", "comps", "shadowData", "subSounds", "grains",
    "children", "doers", "filter", "thingDefs", "categories",
    "disallowedCategories",
})


@dataclass
class Entry:
    kind: str         # "keyed" or "def"
    section: str      # filename (keyed) or DefType (def)
    key: str          # XML tag (keyed) or "defName.field.path" (def)
    source: str
    translation: str = ""


# ---------------------------------------------------------------------------
# entry point
# ---------------------------------------------------------------------------

def main() -> None:
    parser = argparse.ArgumentParser(description="EnhancedIdeology translation helper")
    subs = parser.add_subparsers(dest="cmd", required=True)

    ex = subs.add_parser("extract", help="Extract strings to a markdown template")
    ex.add_argument("--source", default="English")
    ex.add_argument("--target", default="")
    ex.add_argument("--output", help="Output path (default: translation_{target}.md)")
    ex.add_argument("--author", default="", help="Credit for the translator (or source author for English)")
    ex.add_argument("--prefill", action="store_true",
                    help="Pre-fill translation column from existing target-language files")

    gen = subs.add_parser("generate", help="Generate XML from a filled template")
    gen.add_argument("--input", required=True)
    gen.add_argument("--target", help="Override target language from template header")

    args = parser.parse_args()
    {"extract": cmd_extract, "generate": cmd_generate}[args.cmd](args)


# ---------------------------------------------------------------------------
# extract
# ---------------------------------------------------------------------------

def cmd_extract(args) -> None:
    entries = _load_keyed(args.source) + _load_defs()

    existing: dict[str, str] = {}
    if args.prefill and args.target:
        existing = _load_existing(args.target)
    for entry in entries:
        entry.translation = existing.get(entry.key, "")

    TRANSLATIONS_DIR.mkdir(exist_ok=True)
    out = Path(args.output) if args.output else TRANSLATIONS_DIR / f"translation_{args.target or 'template'}.md"
    _write_md(entries, args.source, args.target, out, author=args.author)
    print(f"wrote {out}", file=sys.stderr)


def _load_keyed(lang: str) -> list[Entry]:
    keyed_dir = LANGS_DIR / lang / "Keyed"
    entries: list[Entry] = []
    if not keyed_dir.is_dir():
        return entries
    for path in sorted(keyed_dir.glob("*.xml")):
        try:
            root = ET.parse(path).getroot()
        except ET.ParseError as exc:
            print(f"warning: {path}: {exc}", file=sys.stderr)
            continue
        for child in root:
            if callable(child.tag):
                continue
            text = (child.text or "").strip()
            if text:
                entries.append(Entry("keyed", path.name, child.tag, text))
    return entries


def _load_defs() -> list[Entry]:
    entries: list[Entry] = []
    for path in sorted(DEFS_DIR.rglob("*.xml")):
        try:
            root = ET.parse(path).getroot()
        except ET.ParseError as exc:
            print(f"warning: {path}: {exc}", file=sys.stderr)
            continue
        if root.tag != "Defs":
            continue
        for def_elem in root:
            if callable(def_elem.tag):
                continue
            if def_elem.get("Abstract") == "True":
                continue
            def_type = def_elem.tag
            name_elem = def_elem.find("defName")
            if name_elem is None or not (name_elem.text or "").strip():
                continue
            def_name = name_elem.text.strip()
            for rel_path, text in _def_fields(def_elem):
                entries.append(Entry("def", def_type, f"{def_name}.{rel_path}", text))
    return entries


def _def_fields(def_elem) -> list[tuple[str, str]]:
    results: list[tuple[str, str]] = []
    for child in def_elem:
        if callable(child.tag) or child.tag == "defName":
            continue
        _walk(child, child.tag, results)
    return results


def _walk(elem, path: str, results: list[tuple[str, str]]) -> None:
    tag = elem.tag
    if callable(tag) or tag in SKIP_TAGS:
        return

    if tag in LEAF_FIELDS:
        text = (elem.text or "").strip()
        if text:
            results.append((path, text))
        return

    if tag == "stages":
        for ii, li in enumerate(elem.findall("li")):
            for field_name in ("label", "description"):
                child = li.find(field_name)
                if child is not None:
                    text = (child.text or "").strip()
                    if text:
                        results.append((f"{path}.{ii}.{field_name}", text))
        return

    if tag == "rulesStrings":
        for ii, li in enumerate(elem.findall("li")):
            text = (li.text or "").strip()
            if text:
                results.append((f"{path}.{ii}", text))
        return

    for child in elem:
        if not callable(child.tag):
            _walk(child, f"{path}.{child.tag}", results)


def _load_existing(lang: str) -> dict[str, str]:
    out: dict[str, str] = {}
    for path in (LANGS_DIR / lang).rglob("*.xml"):
        try:
            root = ET.parse(path).getroot()
        except ET.ParseError:
            continue
        for child in root:
            if callable(child.tag):
                continue
            text = (child.text or "").strip()
            if text:
                out[child.tag] = text
    return out


# ---------------------------------------------------------------------------
# markdown I/O
# ---------------------------------------------------------------------------

def _esc(text: str) -> str:
    return text.replace("|", "\\|")


def _unesc(text: str) -> str:
    return text.replace("\\|", "|")


def _write_md(entries: list[Entry], source: str, target: str, path: Path, author: str = "") -> None:
    lines = [
        f"<!-- EnhancedIdeology Translation Template: {source} → {target or 'TODO'} -->",
        f"<!-- Author: {author or 'claude'} -->",
        "<!-- Pipes in text: use \\| -->",
        "",
    ]

    keyed: dict[str, list[Entry]] = defaultdict(list)
    defs: dict[str, list[Entry]] = defaultdict(list)
    for entry in entries:
        (keyed if entry.kind == "keyed" else defs)[entry.section].append(entry)

    for filename, section_entries in keyed.items():
        lines += [f"## Keyed/{filename}", "",
                  "| key | source | translation |",
                  "|-----|--------|-------------|"]
        for ee in section_entries:
            lines.append(f"| `{ee.key}` | {_esc(ee.source)} | {_esc(ee.translation)} |")
        lines.append("")

    for def_type, section_entries in defs.items():
        lines += [f"## DefInjected/{def_type}", "",
                  "| key | source | translation |",
                  "|-----|--------|-------------|"]
        for ee in section_entries:
            lines.append(f"| `{ee.key}` | {_esc(ee.source)} | {_esc(ee.translation)} |")
        lines.append("")

    path.write_text("\n".join(lines), encoding="utf-8")


def _parse_md(path: Path) -> tuple[str, str, str, list[Entry]]:
    text = path.read_text(encoding="utf-8")
    source, target = "English", ""
    mm = re.search(r":\s*(\w+)\s*→\s*(\w+)", text)
    if mm:
        source, target = mm.group(1), mm.group(2)
    author = ""
    am = re.search(r"<!--\s*Author:\s*(.+?)\s*-->", text)
    if am:
        author = am.group(1)

    entries: list[Entry] = []
    kind = ""
    section = ""
    for line in text.splitlines():
        ss = line.strip()
        if ss.startswith("## Keyed/"):
            kind, section = "keyed", ss[9:]
        elif ss.startswith("## DefInjected/"):
            kind, section = "def", ss[15:]
        elif kind and ss.startswith("|") and "| key |" not in ss and not re.match(r"\|[-| ]+\|", ss):
            cols = [cc.strip() for cc in ss.strip("|").split("|")]
            if len(cols) >= 3:
                key = cols[0].strip("`")
                translation = _unesc(cols[2])
                if translation:
                    entries.append(Entry(kind, section, key, _unesc(cols[1]), translation))

    return source, target, author, entries


# ---------------------------------------------------------------------------
# generate
# ---------------------------------------------------------------------------

def cmd_generate(args) -> None:
    source, target, author, entries = _parse_md(Path(args.input))
    if args.target:
        target = args.target
    if not target:
        sys.exit("error: no target language in template; use --target")

    translated = [ee for ee in entries if ee.translation.strip()]
    if not translated:
        print("warning: no translations found in template", file=sys.stderr)
        return

    lang_dir = LANGS_DIR / target
    _write_keyed(translated, lang_dir, author)
    _write_definjected(translated, lang_dir, author)
    print(f"generated in {lang_dir}", file=sys.stderr)


def _xml_esc(text: str) -> str:
    return text.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")


def _xml_header(author: str) -> list[str]:
    lines = ['<?xml version="1.0" encoding="utf-8"?>']
    if author:
        lines.append(f"<!-- Author: {author} -->")
    return lines


def _write_keyed(entries: list[Entry], lang_dir: Path, author: str = "") -> None:
    by_file: dict[str, list[Entry]] = defaultdict(list)
    for ee in entries:
        if ee.kind == "keyed":
            by_file[ee.section].append(ee)

    keyed_dir = lang_dir / "Keyed"
    keyed_dir.mkdir(parents=True, exist_ok=True)
    for filename, file_entries in by_file.items():
        out = keyed_dir / filename
        lines = _xml_header(author) + ["<LanguageData>"]
        for ee in file_entries:
            lines.append(f"  <{ee.key}>{_xml_esc(ee.translation)}</{ee.key}>")
        lines += ["</LanguageData>", ""]
        out.write_text("\n".join(lines), encoding="utf-8")
        print(f"  Keyed/{filename}", file=sys.stderr)


def _write_definjected(entries: list[Entry], lang_dir: Path, author: str = "") -> None:
    by_type: dict[str, list[Entry]] = defaultdict(list)
    for ee in entries:
        if ee.kind == "def":
            by_type[ee.section].append(ee)

    for def_type, type_entries in by_type.items():
        out_dir = lang_dir / "DefInjected" / def_type
        out_dir.mkdir(parents=True, exist_ok=True)
        out = out_dir / f"{def_type}.xml"
        lines = _xml_header(author) + ["<LanguageData>"]
        for ee in type_entries:
            lines.append(f"  <{ee.key}>{_xml_esc(ee.translation)}</{ee.key}>")
        lines += ["</LanguageData>", ""]
        out.write_text("\n".join(lines), encoding="utf-8")
        print(f"  DefInjected/{def_type}/{def_type}.xml", file=sys.stderr)


if __name__ == "__main__":
    main()
