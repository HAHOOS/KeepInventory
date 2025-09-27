using Tomlet.Attributes;

using System.Collections.Generic;

namespace KeepInventory.Saves.V0
{
    public class Save
    {
        [TomlPrecedingComment("The version of the save")]
        public readonly int Version = 0;

        [TomlPrecedingComment("The amount of light ammo left")]
        public int AmmoLight { get; set; }

        [TomlPrecedingComment("The amount of medium ammo left")]
        public int AmmoMedium { get; set; }

        [TomlPrecedingComment("The amount of heavy ammo left")]
        public int AmmoHeavy { get; set; }

        [TomlPrecedingComment("List of all slots & the spawnables stored in them")]
        public Dictionary<string, string> ItemSlots { get; set; } = [];

        public Save()
        { }
    }
}