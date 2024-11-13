using System.Collections.Generic;
using Tomlet.Attributes;

namespace KeepInventory
{
    public class Save
    {
        [TomlPrecedingComment("The amount of light ammo left")]
        public int LightAmmo;

        [TomlPrecedingComment("The amount of medium ammo left")]
        public int MediumAmmo;

        [TomlPrecedingComment("The amount of heavy ammo left")]
        public int HeavyAmmo;

        [TomlPrecedingComment("List of all slots & the spawnables stored in them")]
        public List<SaveSlot.SaveSlot> InventorySlots = [];

        public Save()
        { }
    }
}