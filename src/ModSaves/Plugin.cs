using System;
using System.IO;
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

    /// <summary>
    /// The Xbox runtime's managed API, by assembly-qualified name: the name alone finds it only
    /// once something has loaded its assembly, and at plugin load nothing has (seen on FINAL
    /// FANTASY 1.2.1.0, Xbox app: the plain lookup failed and the plugin fell back to Steam mode).
    /// </summary>
    private const string XboxRuntimeType = "XGamingRuntime.SDK, XGamingRuntime";

    /// <summary>The Xbox runtime's generated assembly, present only in the Xbox app builds.</summary>
    private const string XboxRuntimeAssemblyFile = "XGamingRuntime.dll";

    internal static ManualLogSource Logger = null!;

    /// <summary>The save set for this start, fixed when the plugin loads.</summary>
    internal static string SaveSet { get; private set; } = string.Empty;

    public override void Load()
    {
        Logger = Log;
        ConfigEntry<string> entry = Config.Bind(
            "Saves",
            "SaveSet",
            DefaultSaveSet,
            "Name of the separate save set (letters and digits). Steam: a subfolder of the save folder. Xbox "
                + "app: a prefix on every cloud-save container. Use a different name per mod setup to keep "
                + "several sets apart. Leave empty to use the normal saves. The command-line option "
                + "-modsaves-saveset <name> overrides it for one start."
        );
        string? fromCommandLine = SaveSetNames.FromCommandLine(Environment.GetCommandLineArgs());
        string chosen = fromCommandLine ?? entry.Value ?? string.Empty;
        string? problem = SaveSetNames.Problem(chosen);
        if (problem != null)
        {
            Logger.LogError($"SaveSet {problem}: using \"{DefaultSaveSet}\" instead.");
            chosen = DefaultSaveSet;
        }
        SaveSet = chosen;
        string source = fromCommandLine != null ? " (from the command line)" : string.Empty;
        if (SaveSet.Length == 0)
        {
            Logger.LogInfo($"SaveSet is empty{source}: the game uses its normal saves.");
            return;
        }

        var harmony = new Harmony(Guid);
        harmony.PatchAll(typeof(SteamSaves));
        if (Type.GetType(XboxRuntimeType, throwOnError: false) != null)
        {
            ApplyXbox(harmony);
            Logger.LogInfo($"Xbox app copy: saves go to cloud containers prefixed \"{SaveSet}\"{source}.");
        }
        else if (File.Exists(Path.Combine(Paths.BepInExRootPath, "interop", XboxRuntimeAssemblyFile)))
        {
            Logger.LogError(
                "This is an Xbox app copy, but the Xbox runtime's types could not be loaded: saves are NOT "
                    + "kept apart. Do not save until this is fixed."
            );
        }
        else
        {
            Logger.LogInfo($"Saves go to the \"{SaveSet}\" subfolder of the save folder{source}.");
        }
    }

    /// <summary>
    /// Kept out of Load, and never inlined into it, so a Steam build (which has no XGamingRuntime)
    /// never has to resolve the Xbox types.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ApplyXbox(Harmony harmony) => XboxSaves.Apply(harmony);
}
