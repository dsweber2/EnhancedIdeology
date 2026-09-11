#!/usr/bin/env python3
"""Convert steam-workshop.md to Steam BBCode format."""

import re
import sys

# Map local infographic paths to Steam CDN URLs.
# Fill in each URL after uploading the image via the workshop's image uploader.
IMAGE_URLS: dict[str, str] = {
    # "Infographics/Info_Certainty.png": "https://...",
    # "Infographics/Info_Opinions.png":  "https://...",
    # "Infographics/Info_Books.png":     "https://...",
    # "Infographics/Info_Iconoclasm.png": "https://...",
}

SKIP_HEADINGS: set[str] = set()


def convert(text: str) -> str:
    lines = text.splitlines()
    out = []
    ii = 0
    in_list = False

    def flush_list() -> None:
        nonlocal in_list
        if in_list:
            out.append("[/list]")
            in_list = False

    while ii < len(lines):
        line = lines[ii]

        if line.startswith("# ") and ii == 0:
            ii += 1
            continue

        m = re.match(r"^(#{1,6}) (.+)", line)
        if m:
            heading = m.group(2).strip()

            if heading in SKIP_HEADINGS:
                ii += 1
                while ii < len(lines) and not re.match(r"^#+\s", lines[ii]) and not re.match(r"^---+$", lines[ii].strip()):
                    ii += 1
                continue

            flush_list()
            md_level = len(m.group(1))
            bb_level = max(1, min(md_level - 1, 2))
            tag = f"h{bb_level}"
            out.append(f"[{tag}]{inline(heading)}[/{tag}]")
            ii += 1
            continue

        if re.match(r"^---+$", line.strip()):
            flush_list()
            out.append("[hr][/hr]")
            ii += 1
            continue

        m = re.match(r"^> (.+)", line)
        if m:
            flush_list()
            out.append(f"[small]{inline(m.group(1))}[/small]")
            ii += 1
            continue

        m = re.match(r"^[-*] (.+)", line)
        if m:
            if not in_list:
                out.append("[list]")
                in_list = True
            out.append(f"[*]{inline(m.group(1))}")
            ii += 1
            continue

        flush_list()

        if line.strip() == "":
            out.append("")
            ii += 1
            continue

        out.append(inline(line))
        ii += 1

    flush_list()
    result = "\n".join(out).strip()
    if result.startswith("[hr][/hr]"):
        result = result[len("[hr][/hr]"):].lstrip("\n")
    result = re.sub(r"(\[hr\]\[/hr\]\n*){2,}", "[hr][/hr]\n\n", result)
    return result


def inline(text: str) -> str:
    def replace_image(m: re.Match) -> str:
        src = m.group(2)
        url = IMAGE_URLS.get(src, src)
        return f"[img]{url}[/img]"

    text = re.sub(r"!\[([^\]]*)\]\(([^)]+)\)", replace_image, text)
    text = re.sub(r"\[([^\]]+)\]\(([^)]+)\)", r"[url=\2]\1[/url]", text)
    text = re.sub(r"\*\*\*(.+?)\*\*\*", r"[b][i]\1[/i][/b]", text)
    text = re.sub(r"\*\*(.+?)\*\*", r"[b]\1[/b]", text)
    text = re.sub(r"\*(.+?)\*", r"[i]\1[/i]", text)
    return text


if __name__ == "__main__":
    src = sys.argv[1] if len(sys.argv) > 1 else "steam-workshop.md"
    dst = sys.argv[2] if len(sys.argv) > 2 else "steam-workshop-bbcode.txt"

    with open(src) as ff:
        md = ff.read()

    result = convert(md)

    with open(dst, "w") as ff:
        ff.write(result + "\n")

    print(f"wrote {dst}")
