using System;

using HarmonyLib;

using Il2CppSLZ.Marrow;

using KeepInventory.Managers;
using KeepInventory.Saves.V2;

namespace KeepInventory.Patches
{
    [HarmonyPatch(typeof(AmmoInventory))]
    public static class AmmoInventoryPatches
    {
        public static Save Save { get; set; }
        [HarmonyPatch(nameof(AmmoInventory.Awake))]
        [HarmonyPostfix]
        [HarmonyPriority(0)]
        public static void Awake(AmmoInventory __instance)
        {
            if (Save == null) return;
            if (Core.mp_ammosaving.Value)
            {
                try
                {
                    if (__instance != AmmoInventory.Instance)
                        return;

                    InventoryManager.AddSavedAmmo(Save, Core.mp_showNotifications.Value);
                }
                catch (Exception ex)
                {
                    Core.Logger.Error($"An unexpected error has occurred while attempting to add saved ammo, exception:\n{ex}");
                }
                finally
                {
                    Save = null;
                }
            }
        }
    }
}