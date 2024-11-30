using HarmonyLib;
using Il2CppSLZ.Marrow;

namespace KeepInventory.Patches
{
    [HarmonyPatch(typeof(AmmoInventory))]
    public static class AmmoInventoryPatches
    {
        /// <summary>
        /// Runs after <see cref="AmmoInventory.Awake"/> is run, adds ammo if requested
        /// </summary>
        /// <param name="__instance">Instance of <see cref="AmmoInventory"/></param>
        [HarmonyPatch(nameof(AmmoInventory.Awake))]
        [HarmonyPostfix]
        public static void Awake(AmmoInventory __instance)
        {
            if (!Core.LoadAmmoOnAwake) return;
            if (Core.mp_ammosaving.Value)
            {
                if (__instance != Core.GetAmmoInventory())
                {
                    Core.Logger.Warning("Ammo inventory is not of local player!");
                    return;
                }
                Core.LoadAmmoOnAwake = false;
                Core.AddSavedAmmo(Core.mp_showNotifications.Value);
            }
        }
    }
}