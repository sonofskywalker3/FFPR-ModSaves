# Mod Saves for the FINAL FANTASY Pixel Remasters

Mods that add items, maps, monsters or jobs can leave saves your unmodded game cannot load, and
the bestiary and settings are shared across every slot. On the Xbox app version (Microsoft Store
and PC Game Pass) it is worse: saves sync to the cloud, so a modded save can turn up on your Xbox
console.

Mod Saves gives modded play its own set of saves: its own slots, bestiary and settings. Your normal saves stay exactly as they were, and the game simply does not see them
while the plugin is installed. Remove the plugin and your normal saves are back; the modded set
stays stored, ready for the next time.

Written for all six games (I-VI), which share one engine; so far tested on FINAL FANTASY (I) only.

| Version of the game | How it works | Status |
| --- | --- | --- |
| Xbox app (Microsoft Store, PC Game Pass) | Every cloud-save container gets the save set's name in front (`Slot1` is stored as `ModSlot1`, `System` as `ModSystem`), and the game is shown only that set's containers. The console's unmodded game opens its containers by their own names, so it never sees the modded ones. | Tested on FINAL FANTASY 1.2.1.0: empty load screen with the plugin, modded save stored apart, the Xbox console showed only the normal save |
| Steam | The save folder gets the save set's name as a subfolder. | **Untested.** If you play on Steam, a report is very welcome (see below) |

## Installing

1. Install [BepInEx 6.0.0-pre.2](https://github.com/BepInEx/BepInEx/releases/tag/v6.0.0-pre.2)
   (`BepInEx-Unity.IL2CPP-win-x64`) into the game's folder, the same build Magicite uses, and start
   the game once.
   - Steam: `steamapps\common\FINAL FANTASY PR` (and so on for each game).
   - Xbox app: `C:\XboxGames\FINAL FANTASY\Content` (and so on), or wherever the Xbox app installed it.
2. Copy `ModSaves.dll` into the game's `BepInEx\plugins` folder.
3. Start the game. The load screen is empty the first time: that is your new, separate save set.

To go back to your normal saves, delete `ModSaves.dll` (or set `SaveSet` to nothing, below).

## Settings

`BepInEx\config\sonofskywalker3.ffpr.modsaves.cfg`, written on the first start:

| Setting | Default | What it does |
| --- | --- | --- |
| `SaveSet` | `Mod` | The save set's name. Use a different name for each mod setup to keep several sets apart, for example `Rando` for a randomizer and `Mod` for everything else. Empty means the normal saves. |

## Steam players: please report

I only have the Xbox app version, so the Steam side has not been tried. To check it: install the
plugin, start the game, and look at the load screen. If it is empty, save once, quit, and look in
your save folder (`Documents\My Games\FINAL FANTASY PR\Steam\<number>\` for FF1) for a new `Mod`
folder holding the save. Then let me know in an issue, with which game, whether it worked, and
`BepInEx\LogOutput.log` if it did not.

## Building

Needs the .NET SDK and an Xbox app copy of any of the games with BepInEx run once in it (the build
references its generated `BepInEx\interop` assemblies, including the Xbox runtime's):

```
dotnet build src/ModSaves -c Release -p:GameDir="C:\XboxGames\FINAL FANTASY\Content"
dotnet test tests/ModSaves.Tests
```

## Credits

- The Steam method, adding a folder to the path `SaveSlotManager.GetSavePath` returns, follows
  Silvris's [FFPR-SaveIsolation](https://github.com/Silvris/FFPR-SaveIsolation) showcase. That
  repository has no licence, so this is a separate implementation rather than a fork; I'd gladly
  have built on it directly.
- [BepInEx](https://github.com/BepInEx/BepInEx), [Il2CppInterop](https://github.com/BepInEx/Il2CppInterop)
  and [Harmony](https://github.com/pardeike/Harmony), which every Pixel Remaster plugin stands on.

## AI disclosure

I design my mods and test them, but I'm not a programmer: the code and these docs were written
with Claude Code (Anthropic), under my direction, and checked by me in game. Mod Saves contains no artwork and
no game content.

## Licence

MIT, see [LICENSE](LICENSE). Not affiliated with or endorsed by Square Enix.
