using System;

namespace ModSaves;

/// <summary>
/// How a save set's name is applied to the game's own save locations. Kept free of game and
/// BepInEx types so it can be tested on its own.
/// </summary>
public static class SaveSetNames
{
    /// <summary>
    /// Steam: the folder the game saves in, with the save set's subfolder added
    /// ("C:/.../Steam/123/" becomes "C:/.../Steam/123/Mod/"). An empty set changes nothing.
    /// </summary>
    public static string SteamPath(string gamePath, string saveSet)
    {
        if (string.IsNullOrEmpty(saveSet) || gamePath == null)
        {
            return gamePath!;
        }
        string separator = gamePath.EndsWith("/", StringComparison.Ordinal)
            || gamePath.EndsWith("\\", StringComparison.Ordinal)
            || gamePath.Length == 0
            ? string.Empty
            : "/";
        return gamePath + separator + saveSet + "/";
    }

    /// <summary>
    /// Xbox app: the cloud-save container the game asked for, under the save set's prefix
    /// ("Slot1" becomes "ModSlot1"). Names that already carry the prefix are left alone.
    /// </summary>
    public static string ToStored(string containerName, string saveSet)
    {
        if (string.IsNullOrEmpty(saveSet) || containerName == null || IsInSet(containerName, saveSet))
        {
            return containerName!;
        }
        return saveSet + containerName;
    }

    /// <summary>The name the game uses for a stored container ("ModSlot1" back to "Slot1").</summary>
    public static string ToGame(string storedName, string saveSet)
    {
        if (!IsInSet(storedName, saveSet))
        {
            return storedName;
        }
        return storedName.Substring(saveSet.Length);
    }

    /// <summary>Whether a stored container belongs to the save set.</summary>
    public static bool IsInSet(string storedName, string saveSet)
    {
        return !string.IsNullOrEmpty(saveSet)
            && storedName != null
            && storedName.StartsWith(saveSet, StringComparison.Ordinal);
    }
}
