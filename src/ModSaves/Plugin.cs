using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace ModSaves;

/// <summary>
/// Gives modded play its own saves in the FINAL FANTASY Pixel Remasters.
///
/// Steam copies save to files: the save folder gets the save set's name as a subfolder.
/// Xbox app (Microsoft Store and PC Game Pass) copies save to cloud containers that sync to the
/// Xbox console: every container gets the save set's name in front, so the modded saves live in
/// containers the console's unmodded game never opens.
/// </summary>
[BepInPlugin(Guid, Name, Version)]
public sealed class Plugin : BasePlugin
{
    public const string Guid = "sonofskywalker3.ffpr.modsaves";
    public const string Name = "Mod Saves";
    public const string Version = "0.1.0";
    public const string DefaultSaveSet = "Mod";

    /// <summary>The Xbox runtime's managed API; present only in the Xbox app builds.</summary>
    private const string XboxRuntimeType = "XGamingRuntime.SDK";

    internal static ManualLogSource Logger = null!;
    internal static ConfigEntry<string> SaveSetEntry = null!;

    internal static string SaveSet => SaveSetEntry.Value ?? string.Empty;

    public override void Load()
    {
        Logger = Log;
        SaveSetEntry = Config.Bind(
            "Saves",
            "SaveSet",
            DefaultSaveSet,
            "Name of the separate save set. Steam: a subfolder of the save folder. Xbox app: a prefix on every "
                + "cloud-save container. Use a different name per mod setup to keep several sets apart. Leave "
                + "empty to use the normal saves."
        );
        if (SaveSet.Length == 0)
        {
            Logger.LogInfo("SaveSet is empty: the game uses its normal saves.");
            return;
        }

        var harmony = new Harmony(Guid);
        harmony.PatchAll(typeof(SteamSaves));
        if (AccessTools.TypeByName(XboxRuntimeType) != null)
        {
            ApplyXbox(harmony);
            Logger.LogInfo($"Xbox app copy: saves go to cloud containers prefixed \"{SaveSet}\".");
        }
        else
        {
            Logger.LogInfo($"Saves go to the \"{SaveSet}\" subfolder of the save folder.");
        }
    }

    /// <summary>
    /// Kept out of Load, and never inlined into it, so a Steam build (which has no XGamingRuntime)
    /// never has to resolve the Xbox types.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ApplyXbox(Harmony harmony) => XboxSaves.Apply(harmony);
}
