# Legacy client reference assemblies

The FiveM client runs C# with its own, trimmed copy of `mscorlib`, `System` and `System.Core`. Many .NET Framework members are missing from it or are internal, and calling one only fails in game, once that code runs.

These three files describe what that copy offers, and nothing else. They contain no code, only the public API. The Legacy client projects compile against them, so using something the client does not have becomes a build error. `tools/LegacyChecks --check-refs` reads them too.

## Regenerating them

Only needed when FiveM updates these libraries. Install [Refasmer](https://github.com/JetBrains/Refasmer), then run this from the repository root:

```
refasmer -p --omit-non-api-members=true -O dependencies/legacy/client-ref %LOCALAPPDATA%\FiveM\FiveM.app\citizen\clr2\lib\mono\4.5\mscorlib.dll %LOCALAPPDATA%\FiveM\FiveM.app\citizen\clr2\lib\mono\4.5\System.dll %LOCALAPPDATA%\FiveM\FiveM.app\citizen\clr2\lib\mono\4.5\System.Core.dll
```
