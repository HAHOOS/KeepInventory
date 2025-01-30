using System.Collections.Generic;
using System.Linq;

using Il2CppSLZ.Marrow;

namespace KeepInventory.Helper
{
    /// <summary>
    /// Helper for <see cref="Inventory"/>
    /// </summary>
    public static class InventoryHelper
    {
        /// <summary>
        /// Aliases of slots, key is original name, value is the alias
        /// </summary>
        public static readonly Dictionary<string, string> Aliases = new() {
            {"HeadSlotContainer", "Head" }
        };

        /// <summary>
        /// Get all of the slots of the <see cref="RigManager"/>
        /// </summary>
        /// <param name="rigManager">The <see cref="RigManager"/> to check</param>
        /// <returns>All found slots in the <see cref="RigManager"/></returns>
        public static List<InventorySlotReceiver> GetAllSlots(this RigManager rigManager)
        {
            return rigManager?.GetComponentsInChildren<InventorySlotReceiver>().ToList();
        }

        /// <summary>
        /// Find a slot with provided name
        /// </summary>
        /// <param name="rigManager"><see cref="RigManager"/> to get the slots from</param>
        /// <param name="name">Name of the <see cref="InventorySlotReceiver"/></param>
        /// <returns>If found, returns the <see cref="InventorySlotReceiver"/>, otherwise <see langword="null"/></returns>
        public static InventorySlotReceiver FindSlot(this RigManager rigManager, string name)
        {
            var slots = rigManager.GetAllSlots();
            if (slots == null) return null;
            foreach (var slot in slots)
            {
                if (slot == null) continue;
                if (slot.GetSlotName() == name) return slot;
                else if (slot.GetSlotName(false) == name) return slot;
            }
            return null;
        }

        /// <summary>
        /// Get name of a slot
        /// </summary>
        /// <param name="slotReceiver">The <see cref="InventorySlotReceiver"/> to get name of</param>
        /// <returns>Slot name of the provided <see cref="InventorySlotReceiver"/></returns>
        public static string GetSlotName(this InventorySlotReceiver slotReceiver, bool useAliases = true)
        {
            var name = slotReceiver.transform.parent.name;
            if (name.StartsWith("prop")) name = slotReceiver.transform.parent.parent.name;
            if (Aliases.ContainsKey(name) && useAliases) name = Aliases[name];
            return name;
        }
    }
}