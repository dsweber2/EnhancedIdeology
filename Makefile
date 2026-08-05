.PHONY: build deploy test logs

build:
	dotnet build Source/EnhancedBeliefs/EnhancedBeliefs.csproj

test:
	dotnet test simulator/EnhancedBeliefs.Tests/EnhancedBeliefs.Tests.csproj

logs:
	@mkdir -p simulator/cache
	cp "$(RIMWORLD_LOG)" "simulator/cache/$$(date +%Y%m%d_%H%M%S)_logs.txt"

deploy:
	rsync -a --delete About/      $(RIMWORLD_MOD)/About/
	rsync -a --delete Common/     $(RIMWORLD_MOD)/Common/
	rsync -a --delete Source/     $(RIMWORLD_MOD)/Source/
	rsync -a --delete 1.6/        $(RIMWORLD_MOD)/1.6/
	rsync -a --delete Royalty/    $(RIMWORLD_MOD)/Royalty/
	rsync -a --delete LICENSE     $(RIMWORLD_MOD)/LICENSE
	rsync -a --delete About/      $(WORKSHOP_MOD)/About/
	rsync -a --delete Common/     $(WORKSHOP_MOD)/Common/
	rsync -a --delete 1.6/        $(WORKSHOP_MOD)/1.6/
	rsync -a --delete Royalty/    $(WORKSHOP_MOD)/Royalty/
