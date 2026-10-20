.PHONY: build deploy release test logs preview preview-all steam-page check-translations trait-links catalog

build:
	dotnet build Source/EnhancedIdeology.csproj -p:RimWorldVersion=1.6
	dotnet build Source/EnhancedIdeology.csproj -p:RimWorldVersion=1.7

test: build
	dotnet test tests/EnhancedIdeology.Tests.csproj

logs:
	@mkdir -p cache
	cp "$(RIMWORLD_LOG)" "cache/$$(date +%Y%m%d_%H%M%S)_logs.txt"

# Usage: make preview SVG=images/foo.svg
preview:
	python3 scripts/svg_to_png.py $(SVG)

preview-all:
	@for svg in images/*.svg; do python3 scripts/svg_to_png.py "$$svg"; done

check-translations:
	uv run scripts/translate.py check

catalog:
	uv run scripts/meme_precept_catalog.py

trait-links:
	uv run scripts/trait_links_patch.py

steam-page:
	uv run scripts/md_to_bbcode.py

release:
	../release.sh Source/EnhancedIdeology.csproj $(ARGS)

deploy:
	rsync -a --delete About/      $(RIMWORLD_MOD)/About/
	rsync -a --delete Common/     $(RIMWORLD_MOD)/Common/
	rsync -a --delete Source/     $(RIMWORLD_MOD)/Source/
	rsync -a --delete 1.6/        $(RIMWORLD_MOD)/1.6/
	rsync -a --delete 1.7/        $(RIMWORLD_MOD)/1.7/
	rsync -a --delete Royalty/    $(RIMWORLD_MOD)/Royalty/
	rsync -a --delete LICENSE     $(RIMWORLD_MOD)/LICENSE
	rsync -a LoadFolders.xml      $(RIMWORLD_MOD)/LoadFolders.xml
	rsync -a Analyzer.xml         $(RIMWORLD_MOD)/Analyzer.xml
