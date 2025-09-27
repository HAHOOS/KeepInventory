using Tomlet.Attributes;

using System.Collections.Generic;

namespace KeepInventory.Saves.V1
{
    public class Save
    {
        [TomlPrecedingComment("The version of the save")]
        public readonly int Version = 1;

        [TomlPrecedingComment("The amount of light ammo left")]
        public int LightAmmo { get; set; }

        [TomlPrecedingComment("The amount of medium ammo left")]
        public int MediumAmmo { get; set; }

        [TomlPrecedingComment("The amount of heavy ammo left")]
        public int HeavyAmmo { get; set; }

        [TomlPrecedingComment("List of all slots & the spawnables stored in them")]
        public List<SaveSlot> InventorySlots { get; set; } = [];

        public Save()
        { }

        public Save(Save old)
        {
            LightAmmo = old.LightAmmo;
            MediumAmmo = old.MediumAmmo;
            HeavyAmmo = old.HeavyAmmo;
            InventorySlots = [.. old.InventorySlots];
        }
    }
}