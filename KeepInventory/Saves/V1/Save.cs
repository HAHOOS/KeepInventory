using System.Collections.Generic;

using Tomlet.Attributes;

namespace KeepInventory.Saves.V1
{
    /// <summary>
    /// Class that gets serialized or deserialized, holds all saved info about inventory, ammo etc.
    /// </summary>
    public class Save
    {
        /// <summary>
        /// The version of the save
        /// </summary>
        [TomlPrecedingComment("The version of the save")]
        public readonly int Version = 1;

        /// <summary>
        /// The amount of light ammo left
        /// </summary>
        [TomlPrecedingComment("The amount of light ammo left")]
        public int LightAmmo;

        /// <summary>
        /// The amount of medium ammo left
        /// </summary>
        [TomlPrecedingComment("The amount of medium ammo left")]
        public int MediumAmmo;

        /// <summary>
        /// The amount of heavy ammo left
        /// </summary>
        [TomlPrecedingComment("The amount of heavy ammo left")]
        public int HeavyAmmo;

        /// <summary>
        /// List of all slots and the spawnables stored in them
        /// </summary>
        [TomlPrecedingComment("List of all slots & the spawnables stored in them")]
        public List<SaveSlot> InventorySlots = [];

        /// <summary>
        /// Create new instance of <see cref="Save"/>
        /// </summary>
        public Save()
        { }

        /// <summary>
        /// Create new instance of <see cref="Save"/> from an old one
        /// </summary>
        public Save(Save old)
        {
            LightAmmo = old.LightAmmo;
            MediumAmmo = old.MediumAmmo;
            HeavyAmmo = old.HeavyAmmo;
            InventorySlots = new List<SaveSlot>(old.InventorySlots);
        }
    }
}