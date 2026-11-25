using ModSaves;
using Xunit;

namespace ModSaves.Tests;

public class SaveSetNamesTests
{
    [Theory]
    [InlineData("C:/Saves/Steam/1/", "Mod", "C:/Saves/Steam/1/Mod/")]
    [InlineData("C:/Saves/Steam/1", "Mod", "C:/Saves/Steam/1/Mod/")]
    [InlineData("C:\\Saves\\Steam\\1\\", "Mod", "C:\\Saves\\Steam\\1\\Mod/")]
    [InlineData("C:/Saves/Steam/1/", "", "C:/Saves/Steam/1/")]
    public void SteamPath_adds_the_save_set_folder(string gamePath, string saveSet, string expected)
    {
        Assert.Equal(expected, SaveSetNames.SteamPath(gamePath, saveSet));
    }

    [Theory]
    [InlineData("Slot1", "Mod", "ModSlot1")]
    [InlineData("System", "Mod", "ModSystem")]
    [InlineData("ModSlot1", "Mod", "ModSlot1")]
    [InlineData("Slot1", "", "Slot1")]
    [InlineData("", "Mod", "Mod")]
    public void ToStored_prefixes_container_names_once(string name, string saveSet, string expected)
    {
        Assert.Equal(expected, SaveSetNames.ToStored(name, saveSet));
    }

    [Theory]
    [InlineData("ModSlot1", "Mod", true, "Slot1")]
    [InlineData("Slot1", "Mod", false, "Slot1")]
    [InlineData("RandoSlot1", "Mod", false, "RandoSlot1")]
    [InlineData("Slot1", "", false, "Slot1")]
    public void Stored_names_map_back_only_within_the_set(string stored, string saveSet, bool inSet, string gameName)
    {
        Assert.Equal(inSet, SaveSetNames.IsInSet(stored, saveSet));
        Assert.Equal(gameName, SaveSetNames.ToGame(stored, saveSet));
    }
}
