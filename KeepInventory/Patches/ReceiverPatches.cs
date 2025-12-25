using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BoneLib;

using HarmonyLib;

using Il2CppSLZ.Marrow;

using KeepInventory.Helper;

namespace KeepInventory.Patches
{
    [HarmonyPatch(typeof(InventorySlotReceiver))]
    public static class ReceiverPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventorySlotReceiver.OnHandDrop))]
        public static void Postfix(InventorySlotReceiver __instance)
        {
            if (!Core.HasFusion || !Utilities.Fusion.IsConnected)
                return;

            if (__instance.GetComponentInParent<RigManager>() != Player.RigManager)
                return;

            if (!InventoryHelper.Callback.TryGetValue(__instance, out var action))
                return;

            action?.Invoke();
        }
    }
}
