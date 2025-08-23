using System;

using HarmonyLib;

using Il2CppSLZ.Marrow;

using KeepInventory.Managers;
using KeepInventory.Saves.V2;

namespace KeepInventory.Patches
{
    [HarmonyPatch(typeof(AmmoInventory), nameof(AmmoInventory.Awake))]
    public static class AmmoInventoryPatches
    {
        public static Save Save { get; set; }

        public static void Postfix(AmmoInventory __instance)
        {
            if (Save == null)
                return;

            if (!PreferencesManager.AmmoSaving.Value)
                return;

            if (__instance != AmmoInventory.Instance)
                return;

            try
            {
                InventoryManager.AddSavedAmmo(Save);
            }
            catch (Exception ex)
            {
                Core.Logger.Error("An unexpected error has occurred while attempting to add saved ammo", ex);
            }
            finally
            {
                Save = null;
            }
        }
    }
}