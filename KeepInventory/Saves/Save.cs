using System.Collections.Generic;

using Tomlet.Attributes;

namespace KeepInventory.Saves
{
    /// <summary>
    /// Class that gets serialized or deserialized, holds all saved info about inventory, ammo etc.
    /// </summary>
    public class Save
    {
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
        /// Create new instance of <see cref="Save"/>, exists for JSON deserializing
        /// </summary>
        public Save()
        { }
    }
}