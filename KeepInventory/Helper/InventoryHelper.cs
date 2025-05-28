using System.Collections.Generic;
using System.Linq;

using Il2CppSLZ.Marrow;

namespace KeepInventory.Helper
{
    public static class InventoryHelper
    {
        public static readonly Dictionary<string, string> Aliases = new() {
            {"HeadSlotContainer", "Head" }
        };
        public static List<InventorySlotReceiver> GetAllSlots(this RigManager rigManager)
        {
            return rigManager?.GetComponentsInChildren<InventorySlotReceiver>().ToList();
        }
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
        public static string GetSlotName(this InventorySlotReceiver slotReceiver, bool useAliases = true)
        {
            var name = slotReceiver.transform.parent.name;
            if (name.StartsWith("prop")) name = slotReceiver.transform.parent.parent.name;
            if (Aliases.ContainsKey(name) && useAliases) name = Aliases[name];
            return name;
        }
    }
}