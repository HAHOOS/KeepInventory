using System;
using System.Drawing;

using Il2CppSLZ.Marrow;

using KeepInventory.Saves.V2;
using KeepInventory.Utilities;

namespace KeepInventory.Helper
{
    /// <summary>
    /// Provides helper methods for <see cref="Gun"/>
    /// </summary>
    public static class GunHelper
    {
        private readonly static Color SlotColor = Color.Cyan;

        /// <summary>
        /// Updates the gun with provided <see cref="GunInfo"/>
        /// </summary>
        /// <param name="gun">The gun to update</param>
        /// <param name="info">The data to update with</param>
        /// <param name="slot">The save slot (debugging purposes)</param>
        /// <param name="name">Name of the crate (debugging purposes)</param>
        /// <param name="barcode">Barcode of the spawnable (debugging purposes)</param>
        /// <param name="printMessages">If <see langword="true"/>, the method will print debug messages using <see cref="MelonLoader.MelonLogger.Instance"/></param>
        public static void UpdateProperties(this Gun gun, GunInfo info, SaveSlot slot = null, string name = "N/A", string barcode = "N/A", bool printMessages = true)
        {
            if (string.IsNullOrWhiteSpace(name)) name = "N/A";
            if (string.IsNullOrWhiteSpace(barcode)) barcode = "N/A";
            string slotName = slot == null ? "N/A" : string.IsNullOrWhiteSpace(slot.SlotName) ? "N/A" : slot.SlotName;

            if (gun != null && info != null)
            {
                try
                {
                    void other()
                    {
                        if (printMessages) Core.MsgPrefix($"Setting fire mode value to {info.FireMode}", slotName, SlotColor);
                        gun.fireMode = info.FireMode;
                        if (printMessages) Core.MsgPrefix($"Setting 'HasFiredOnce' value to {info.HasFiredOnce}", slotName, SlotColor);
                        gun.hasFiredOnce = info.HasFiredOnce;
                        if (printMessages) Core.MsgPrefix($"Setting hammer state to {info.HammerState}", slotName, SlotColor);
                        gun.hammerState = info.HammerState;
                        if (printMessages) Core.MsgPrefix($"Setting slide state to {info.SlideState}", slotName, SlotColor);
                        gun.slideState = info.SlideState;

                        if (!gun.isCharged && info.IsBulletInChamber)
                        {
                            if (printMessages) Core.MsgPrefix("Charging gun", slotName, SlotColor);
                            gun.Charge();
                            if (gun._hasMagState) gun.MagazineState.AddCartridge(1, gun.defaultCartridge);
                            if (printMessages) Core.MsgPrefix($"Setting cartridge state to {info.CartridgeState}", slotName, SlotColor);
                            gun.cartridgeState = info.CartridgeState;
                        }

                        switch (info.SlideState)
                        {
                            case Gun.SlideStates.LOCKED:
                                gun.SlideLocked();
                                break;

                            case Gun.SlideStates.PULLING:
                                gun.CompleteSlidePull();
                                break;

                            case Gun.SlideStates.RETURNING:
                                gun.CompleteSlideReturn();
                                break;
                        }

                        if (printMessages) Core.MsgPrefix($"Spawned to slot: {name} ({barcode})", slotName, SlotColor);
                    }

                    if (printMessages) Core.MsgPrefix($"Writing gun info for: {name} ({barcode})", slotName, SlotColor);
                    if (info.IsMag && gun.defaultMagazine != null && gun.defaultCartridge != null)
                    {
                        if (printMessages) Core.MsgPrefix("Loading magazine", slotName, SlotColor);
                        var mag = info.GetMagazineData(gun);
                        if (mag?.spawnable?.crateRef != null && !string.IsNullOrWhiteSpace(mag.platform))
                        {
                            gun.LoadMagazine(info.RoundsLeft, other);
                        }
                        else
                        {
                            if (printMessages) Core.Logger.Warning($"[{slotName}] Could not get sufficient information for MagazineData, not loading the magazine and rounds left");
                            other();
                        }
                    }
                    else
                    {
                        other();
                    }
                }
                catch (Exception ex)
                {
                    Core.Logger.Error($"An unexpected error has occurred while applying gun info, exception:\n{ex}");
                    BLHelper.SendNotification("Error", "Failed to successfully apply gun info to the gun, check the console or logs for more info", true, 3f, BoneLib.Notifications.NotificationType.Error);
                }
            }
        }
    }
}