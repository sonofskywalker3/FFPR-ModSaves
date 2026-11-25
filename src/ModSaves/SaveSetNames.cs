using System;

namespace ModSaves;

/// <summary>
/// How a save set's name is applied to the game's own save locations. Kept free of game and
/// BepInEx types so it can be tested on its own.
/// </summary>
public static class SaveSetNames
{
    /// <summary>The command-line option that sets the save set for one start only.</summary>
    public const string CommandLineOption = "-modsaves-saveset";

    /// <summary>The cloud-save containers the Xbox app copies open, by their own names.</summary>
    public static readonly string[] GameContainers = BuildGameContainers();

    private static string[] BuildGameContainers()
    {
        var names = new System.Collections.Generic.List<string> { "System", "Slot" };
        for (int i = 1; i <= 20; i++)
        {
            names.Add("Slot" + i);
        }
        names.Add("Slot22");
        return names.ToArray();
    }

    /// <summary>
    /// The save set named on the command line ("-modsaves-saveset Name" or
    /// "-modsaves-saveset=Name"), or null when there is none. The value is used for that start
    /// only and never written to the config file. A missing or empty value counts as no option, so
    /// a broken command line can never switch the separate saves off.
    /// </summary>
    public static string? FromCommandLine(string[] args)
    {
        if (args == null)
        {
            return null;
        }
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i] ?? string.Empty;
            string? value = null;
            if (arg.Equals(CommandLineOption, StringComparison.OrdinalIgnoreCase))
            {
                value = i + 1 < args.Length ? args[i + 1] : null;
            }
            else if (arg.StartsWith(CommandLineOption + "=", StringComparison.OrdinalIgnoreCase))
            {
                value = arg.Substring(CommandLineOption.Length + 1);
            }
            if (value != null)
            {
                return value.Length == 0 || value.StartsWith("-", StringComparison.Ordinal) ? null : value;
            }
        }
        return null;
    }

    /// <summary>
    /// Why a save set's name cannot be used, or null when it can. Empty is allowed (the normal
    /// saves). Only letters and digits, and never the start of one of the game's own container
    /// names: with "Slot" as the set, "Slot1" would already look like one of the set's containers
    /// and the game would read and write the normal save.
    /// </summary>
    public static string? Problem(string saveSet)
    {
        if (string.IsNullOrEmpty(saveSet))
        {
            return null;
        }
        foreach (char c in saveSet)
        {
            bool plain = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
            if (!plain)
            {
                return $"\"{saveSet}\" has a character other than A-Z, a-z or 0-9";
            }
        }
        foreach (string name in GameContainers)
        {
            if (name.StartsWith(saveSet, StringComparison.Ordinal))
            {
                return $"\"{saveSet}\" is the start of the game's own container name \"{name}\"";
            }
        }
        return null;
    }

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
