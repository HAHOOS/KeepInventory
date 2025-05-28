using BoneLib;

using HarmonyLib;

using Il2CppSLZ.Marrow;

using KeepInventory.Utilities;
using KeepInventory.Helper;

namespace KeepInventory.Patches
{
    [HarmonyPatch(typeof(Player_Health))]
    public static class HealthPatches
    {
        private static void HolsterItem(Hand hand)
        {
            var gameObject = hand.m_CurrentAttachedGO;
            if (gameObject == null)
                return;

            var grip = gameObject.GetComponent<Grip>();
            if (grip == null)
                return;

            var host = grip.Host;

            var slot = host.GetHostGameObject()?.GetComponentInChildren<WeaponSlot>();
            if (slot == null)
                return;

            var bodySlots = Player.RigManager?.GetAllSlots();
            if (bodySlots == null || bodySlots.Count == 0)
                return;

            foreach (var item in bodySlots)
            {
                if (item._slottedWeapon != null)
                    continue;

                if (item.slotType != slot.slotType)
                    continue;

                item.OnHandDrop(host);
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player_Health.Death))]
        public static void Prefix(Player_Health __instance)
        {
            var rigManager = __instance._rigManager;
            if (!rigManager.IsLocalPlayer())
                return;

            if (!Core.mp_holsterHeldWeaponsOnDeath.Value)
                return;

            if (Utilities.Fusion.IsGamemodeStarted)
                return;

            var hand1 = Player.LeftHand;
            var hand2 = Player.RightHand;
            HolsterItem(hand1);
            HolsterItem(hand2);
        }
    }
}