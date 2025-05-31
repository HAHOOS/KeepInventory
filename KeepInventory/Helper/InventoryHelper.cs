using System;
using System.Collections.Generic;
using System.Linq;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;

using UnityEngine;

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

        public static void SpawnInSlot(this InventorySlotReceiver receiver, Barcode barcode, Action<GameObject> callback = null)
        {
            if (receiver == null)
                return;

            if (barcode == null)
                return;

            if (!AssetWarehouse.Instance.HasCrate(barcode))
            {
                Core.Logger.Warning($"Could not spawn item to slot '{receiver.GetSlotName()}', because the barcode does not exist: {barcode.ID}");
                return;
            }

            if (Utilities.Fusion.IsConnected)
            {
                Utilities.Fusion.Fusion_SpawnInSlot(receiver, barcode, callback);
            }
            else
            {
                if (receiver._slottedWeapon?.interactableHost != null)
                {
                    receiver._weaponHost?.ForceDetach();
                    receiver.DropWeapon();
                }
                var task = receiver.SpawnInSlotAsync(barcode);
                var awaiter = task.GetAwaiter();
                awaiter.OnCompleted((Action)(() =>
                {
                    if (awaiter.GetResult())
                        callback?.Invoke(receiver._slottedWeapon.GetComponentInParent<Poolee>()?.gameObject);
                }));
            }
        }
    }
}