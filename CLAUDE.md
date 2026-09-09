# EnhancedBeliefs mod

- build and deploy: `make build && make deploy`. if you build, deploy
- csproj: `Source/EnhancedIdeology.csproj` (namespace is `EnhancedIdeology`; the `EnhancedBeliefs` name is the mod/folder name)
- only one DLL should be in `1.6/Assemblies/`: `EnhancedIdeology.dll` — if `EnhancedBeliefs.dll` appears there, delete it (stale build artifact from an old namespace migration attempt)
- log snapshots: `make logs` → `simulator/cache/<timestamp>_logs.txt`
- run the tests with `make test`
