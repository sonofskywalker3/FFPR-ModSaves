using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using XGamingRuntime;

namespace ModSaves;

/// <summary>
/// Xbox app copies: the game opens its cloud-save containers by fixed name ("System", "Slot",
/// "Slot1" to "Slot20", "Slot22") through the Xbox runtime. Every call that names a container
/// gets the save set's prefix, and container listings show the game only the save set's
/// containers, under their usual names.
///
/// This class touches XGamingRuntime types, which Steam builds do not have: it is only loaded
/// when Plugin has found the runtime.
/// </summary>
internal static class XboxSaves
{
    private static readonly string[] ContainerNameMethods =
    {
        nameof(SDK.XGameSaveCreateContainer),
        nameof(SDK.XGameSaveDeleteContainer),
        nameof(SDK.XGameSaveDeleteContainerAsync),
        nameof(SDK.XGameSaveGetContainerInfo),
    };

    public static void Apply(Harmony harmony)
    {
        var rename = new HarmonyMethod(typeof(XboxSaves), nameof(RenameContainer));
        foreach (MethodInfo method in typeof(SDK).GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            if (Array.IndexOf(ContainerNameMethods, method.Name) >= 0)
            {
                harmony.Patch(method, prefix: rename);
            }
        }
        harmony.Patch(
            AccessTools.Method(typeof(SDK), nameof(SDK.XGameSaveEnumerateContainerInfoByName)),
            prefix: new HarmonyMethod(typeof(XboxSaves), nameof(RenameListingPrefix)),
            postfix: new HarmonyMethod(typeof(XboxSaves), nameof(ShowOnlySaveSet))
        );
        harmony.Patch(
            AccessTools.Method(typeof(SDK), nameof(SDK.XGameSaveEnumerateContainerInfo)),
            postfix: new HarmonyMethod(typeof(XboxSaves), nameof(ShowOnlySaveSet))
        );
    }

    private static void RenameContainer(MethodBase __originalMethod, ref string containerName)
    {
        string original = containerName;
        containerName = SaveSetNames.ToStored(original, Plugin.SaveSet);
        Plugin.Logger.LogDebug($"{__originalMethod.Name}: {original} -> {containerName}");
    }

    private static void RenameListingPrefix(ref string containerNamePrefix)
    {
        containerNamePrefix = SaveSetNames.ToStored(containerNamePrefix ?? string.Empty, Plugin.SaveSet);
    }

    private static void ShowOnlySaveSet(ref Il2CppReferenceArray<XGameSaveContainerInfo> containerInfos)
    {
        if (containerInfos == null)
        {
            return;
        }
        var kept = new List<XGameSaveContainerInfo>();
        foreach (XGameSaveContainerInfo info in containerInfos)
        {
            if (SaveSetNames.IsInSet(info.Name, Plugin.SaveSet))
            {
                info.Name = SaveSetNames.ToGame(info.Name, Plugin.SaveSet);
                kept.Add(info);
            }
        }
        var result = new Il2CppReferenceArray<XGameSaveContainerInfo>(kept.Count);
        for (int i = 0; i < kept.Count; i++)
        {
            result[i] = kept[i];
        }
        containerInfos = result;
    }
}
