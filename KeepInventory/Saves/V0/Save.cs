using System.Collections.Generic;

using Tomlet.Attributes;

namespace KeepInventory.Saves.V0
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
        public readonly int Version = 0;

        /// <summary>
        /// Saved ammo of type Light
        /// </summary>
        [TomlPrecedingComment("The amount of light ammo left")]
        public int AmmoLight;

        /// <summary>
        /// Saved ammo of type Medium
        /// </summary>
        [TomlPrecedingComment("The amount of medium ammo left")]
        public int AmmoMedium;

        /// <summary>
        /// Saved ammo of type Heavy
        /// </summary>
        [TomlPrecedingComment("The amount of heavy ammo left")]
        public int AmmoHeavy;

        /// <summary>
        /// Saved items in the inventory
        /// </summary>
        [TomlPrecedingComment("List of all slots & the spawnables stored in them")]
        public Dictionary<string, string> ItemSlots = [];

        /// <summary>
        /// Create new instance of <see cref="Save"/>
        /// </summary>
        public Save()
        { }
    }
}