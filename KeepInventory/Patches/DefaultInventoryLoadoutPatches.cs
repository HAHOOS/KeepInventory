using HarmonyLib;
using Il2CppSLZ.Bonelab;

namespace KeepInventory.Patches
{
    [HarmonyPatch(typeof(DefaultInventoryLoadout))]
    public static class DefaultInventoryLoadoutPatches
    {
        /// <summary>
        /// Prevents the default (saved) loadout from being loaded
        /// </summary>
        /// <returns>If <see langword="true"/>, continue the execution of the method, otherwise abort</returns>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(DefaultInventoryLoadout.SpawnInSlotsAsync))]
        public static bool Override()
        {
            Core.Logger.Msg("DefaultInventoryLoadout");
            return false;
        }
    }
}