using System.Collections.Generic;

using Tomlet.Attributes;

namespace KeepInventory.Saves.V0
{
    public class Save
    {
        [TomlPrecedingComment("The version of the save")]
        public readonly int Version = 0;
        [TomlPrecedingComment("The amount of light ammo left")]
        public int AmmoLight;
        [TomlPrecedingComment("The amount of medium ammo left")]
        public int AmmoMedium;
        [TomlPrecedingComment("The amount of heavy ammo left")]
        public int AmmoHeavy;
        [TomlPrecedingComment("List of all slots & the spawnables stored in them")]
        public Dictionary<string, string> ItemSlots = [];
        public Save()
        { }
    }
}