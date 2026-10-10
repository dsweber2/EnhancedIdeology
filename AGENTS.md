# EnhancedBeliefs mod

- build and deploy: `make build && make deploy`. if you build, deploy
- csproj: `Source/EnhancedIdeology.csproj` (namespace is `EnhancedIdeology`; the `EnhancedBeliefs` name is the mod/folder name)
- only one DLL should be in `1.6/Assemblies/`: `EnhancedIdeology.dll` — if `EnhancedBeliefs.dll` appears there, delete it (stale build artifact from an old namespace migration attempt)
- log snapshots: `make logs` → `cache/<timestamp>_logs.txt`
- run the tests with `make test`
- regenerate `docs/meme-precept-catalog.md` (all memes and precept issues per installed mod) with `uv run scripts/meme_precept_catalog.py`
- `Common/Patches/TraitLinks.xml` is generated from the `meme/precepts` column of `docs/trait-meme-affinity.md`: edit the doc, then `uv run scripts/trait_links_patch.py`

## Translations

Language files live in `Common/Languages/{Lang}/`. English is the source of truth.

`scripts/translate.py` manages the extract → translate → generate pipeline. Templates live in `translations/`.

```bash
# refresh both standing templates (run this after adding new strings):
uv run scripts/translate.py extract --source English --author dsweber2
uv run scripts/translate.py extract --source English --target "German (Deutsch)" --prefill

# start a brand-new language (--author defaults to "claude"); --target must match the vanilla folder name exactly:
uv run scripts/translate.py extract --source English --target "French (Français)"

# write XML files from a filled-in template:
uv run scripts/translate.py generate --input "translations/translation_French (Français).md"
```

The template is a markdown file with `| key | source | translation |` tables — fill the translation column (human or LLM), then run generate. `--prefill` reads any existing XML files for that language and pre-populates the translation column so prior work is preserved. `--author` embeds a credit line near the top of the template and in the generated XML files.

- Keyed files are written directly to `Languages/{Lang}/Keyed/` with the same filenames as English
- DefInjected files are written to `Languages/{Lang}/DefInjected/{DefType}/{DefType}.xml`
- If hand-written DefInjected files already exist alongside generated ones, RimWorld reads both — delete the old files to avoid duplicate keys
