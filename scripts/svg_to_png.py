#!/usr/bin/env python3
"""Export an SVG to a main PNG and a mask PNG.

The mask PNG only renders layers/groups whose inkscape:label or id contains
'_mask' (case-insensitive). All other layers are hidden before export.

The output path can be embedded in the SVG via a custom attribute:
    <svg xmlns:eb="urn:enhancedbeliefs" eb:export="Common/Textures/Things/Building/Foo.png" ...>

Paths are resolved relative to the repo root (git rev-parse --show-toplevel).
The mask is always written to <stem>m.png next to the main output (RimWorld convention).

Usage:
    svg_to_png.py <input.svg> [<output.png>]

The CLI output path takes precedence over the embedded eb:export attribute.
"""

import sys
import subprocess
import tempfile
from pathlib import Path
from lxml import etree

INKSCAPE_NS = "http://www.inkscape.org/namespaces/inkscape"
EB_NS = "urn:enhancedbeliefs"


def repo_root() -> Path:
    result = subprocess.run(
        ["git", "rev-parse", "--show-toplevel"],
        check=True,
        capture_output=True,
        text=True,
    )
    return Path(result.stdout.strip())


def label(element: etree._Element) -> str:
    return (
        element.get(f"{{{INKSCAPE_NS}}}label", "")
        or element.get("id", "")
    )


def has_mask_descendant(element: etree._Element) -> bool:
    if "_mask" in label(element).lower():
        return True
    return any(has_mask_descendant(child) for child in element)


def _hide(element: etree._Element) -> None:
    style = element.get("style", "")
    parts = [p for p in style.split(";") if p and not p.startswith("display")]
    parts.append("display:none")
    element.set("style", ";".join(parts))


def _apply_mask_visibility(element: etree._Element) -> None:
    if "_mask" in label(element).lower():
        return  # keep this element and all its children
    if has_mask_descendant(element):
        for child in element:
            _apply_mask_visibility(child)
    else:
        _hide(element)


def hide_non_mask_layers(tree: etree._ElementTree) -> None:
    root = tree.getroot()
    for child in root:
        _apply_mask_visibility(child)


def export_png(svg_path: Path, png_path: Path, dpi: int = 96) -> None:
    png_path.parent.mkdir(parents=True, exist_ok=True)
    subprocess.run(
        [
            "inkscape",
            f"--export-filename={png_path}",
            "--export-type=png",
            f"--export-dpi={dpi}",
            str(svg_path),
        ],
        check=True,
        capture_output=True,
    )


def read_embedded_export(src: Path) -> Path | None:
    tree = etree.parse(str(src))
    root = tree.getroot()
    value = root.get(f"{{{EB_NS}}}export")
    if value:
        return repo_root() / value
    return None


def main() -> None:
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)

    src = Path(sys.argv[1])
    if not src.exists():
        print(f"error: {src} not found", file=sys.stderr)
        sys.exit(1)

    if len(sys.argv) > 2:
        out_png = Path(sys.argv[2])
    else:
        out_png = read_embedded_export(src)
        if out_png is None:
            print(f"error: no output path given and no eb:export attribute in {src}", file=sys.stderr)
            sys.exit(1)

    mask_png = out_png.with_name(out_png.stem + "m.png")

    export_png(src, out_png)
    print(f"main: {out_png}")

    tree = etree.parse(str(src))
    hide_non_mask_layers(tree)

    with tempfile.NamedTemporaryFile(suffix=".svg", delete=False) as tmp:
        tmp_path = Path(tmp.name)
        tree.write(str(tmp_path), xml_declaration=True, encoding="UTF-8")

    try:
        export_png(tmp_path, mask_png)
        print(f"mask: {mask_png}")
    finally:
        tmp_path.unlink()


if __name__ == "__main__":
    main()
