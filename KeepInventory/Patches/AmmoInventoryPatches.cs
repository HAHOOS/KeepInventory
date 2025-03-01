using System;

using HarmonyLib;

using Il2CppSLZ.Marrow;

using KeepInventory.Saves.V2;

namespace KeepInventory.Patches
{
    /// <summary>
    /// Class that contains patches for <see cref="AmmoInventory"/>
    /// </summary>
    [HarmonyPatch(typeof(AmmoInventory))]
    public static class AmmoInventoryPatches
    {
        /// <summary>
        /// Save that should be used when AmmoInventory has <see cref="AmmoInventory.Awake"/> triggered
        /// <para>
        /// Setting this to <see langword="null"/> will cause the adding to not be done. When the adding will complete, the save will return to the value of <see langword="null"/>
        /// </para>
        /// </summary>
        public static Save Save { get; set; }

        /// <summary>
        /// Runs after <see cref="AmmoInventory.Awake"/> is run, adds ammo if requested
        /// </summary>
        /// <param name="__instance">Instance of <see cref="AmmoInventory"/></param>
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