using HarmonyLib;
using Last.Management;

namespace ModSaves;

/// <summary>
/// Steam copies: every save file is read and written under the folder SaveSlotManager.GetSavePath
/// returns, so adding the save set's subfolder there moves all of them. (Untested on Steam so
/// far: the method follows Silvris's FFPR-SaveIsolation showcase. On the Xbox app copies the game
/// never calls it.)
/// </summary>
[HarmonyPatch(typeof(SaveSlotManager), nameof(SaveSlotManager.GetSavePath))]
internal static class SteamSaves
{
    private static string? lastLogged;

    private static void Postfix(ref string __result)
    {
        string original = __result;
        __result = SaveSetNames.SteamPath(original, Plugin.SaveSet);
        if (original != lastLogged)
        {
            Plugin.Logger.LogInfo($"Save folder: {original} -> {__result}");
            lastLogged = original;
        }
    }
}
